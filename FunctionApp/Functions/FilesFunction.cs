using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.ObjectMapper;
using OLab.Api.Utils;
using OLab.FunctionApp.Extensions;
using System.Net;

namespace OLab.FunctionApp.Functions
{
  public class FilesFunction : OLabFunction
  {
    private readonly FilesEndpoint _endpoint;

    public FilesFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<FilesFunction>(loggerFactory);
      _endpoint = new FilesEndpoint(Logger, configuration, dbContext);
    }

    public string GetMimeTypeForFileExtension(string filePath)
    {
      const string DefaultContentType = "application/octet-stream";

      var provider = new FileExtensionContentTypeProvider();

      if (!provider.TryGetContentType(filePath, out var contentType))
      {
        contentType = DefaultContentType;
      }

      return contentType;
    }

    private static string CapitalizeFirstLetter(string str)
    {
      if (str.Length == 0)
        return str;

      if (str.Length == 1)
        return char.ToUpper(str[0]).ToString();
      else
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Build static file file name
    /// </summary>
    /// <param name="dto">Files dto</param>
    /// <param name="postedFile">FormData</param>
    /// <returns>Static File name</returns>
    private string BuildStaticFileName(FilesFullDto dto)
    {
      string tempFileName;

      var dirName = Path.Combine(
        CapitalizeFirstLetter(dto.ImageableType),
        dto.ImageableId.ToString());

      tempFileName = Path.Combine(dirName, dto.Name);

      Logger.LogDebug($"Static file name: {tempFileName}");

      return tempFileName;
    }

    /// <summary>
    /// Gets all counters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="Logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("FilesGet")]
    public async Task<HttpResponseData> FilesGetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "files")] HttpRequestData request,
        FunctionContext hostContext,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));

      try
      {
        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var pagedResult = await _endpoint.GetAsync(take, skip);
        Logger.LogInformation(string.Format("Found {0} files", pagedResult.Data.Count));

        response = request.CreateResponse(
          OLabObjectPagedListResult<FilesDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }

    /// <summary>
    /// Gets single constant
    /// </summary>
    /// <param name="id">Counter id</param>
    /// <returns></returns>
    [Function("FileGet")]
    public async Task<HttpResponseData> FileGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "files/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id
    )
    {
      Guard.Argument(request).NotNull(nameof(request));

      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        var blobName = BuildStaticFileName(dto);

        // generate short-lived blob download url
        //var sasGenerator = new AzureStorageBlobOptionsTokenGenerator(_appSettings);
        //dto.Url = sasGenerator.GenerateSasToken(_configuration.GetValue<string>("WebsitePublicFilesDirectory"), blobName);

        response = request.CreateResponse(OLabObjectResult<FilesFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Create new file
    /// </summary>
    /// <param name="dto">File data</param>
    /// <returns>IActionResult</returns>
    [Function("FilePost")]
    public async Task<HttpResponseData> FilePostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "file")] HttpRequestData request,
      FunctionContext hostContext)
    {
      SystemFiles phys = null;

      try
      {
        Logger.LogDebug($"FilePostAsync");

        var parser = await MultipartFormDataParser.ParseAsync(request.Body).ConfigureAwait(false);

        var dto = new FilesFullDto(parser);

        var builder = new FilesFull(Logger);
        phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        var staticFileName = BuildStaticFileName(dto);

        // save just the file name to the database
        phys.Path = Path.GetFileName(staticFileName);
        // infer the mimetype from the file name
        phys.Mime = GetMimeTypeForFileExtension(phys.Name);

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        dto = await _endpoint.PostAsync(auth, phys);

        // TODO: successful save to database, copy the file to the
        // final location
        Stream myBlob = new MemoryStream();
        var file = parser.Files[0];
        //myBlob = file.OpenReadStream();

        //var blobClient = new BlobContainerClient(
        //  _appSettings.StaticFilesConnectionString,
        //  _appSettings.StaticFilesContainerName);

        //var blob = blobClient.GetBlobClient(staticFileName);
        //await blob.UploadAsync(myBlob);

        response = request.CreateResponse(OLabObjectResult<FilesFullDto>.Result(dto));

      }
      catch (Exception ex)
      {
        if (ex is global::Azure.RequestFailedException)
        {
          var azureException = ex as global::Azure.RequestFailedException;
          if (azureException.Status == 409)
            response = request.CreateResponse(
              OLabServerErrorResult.Result(
                $"File '{phys.Path}' already exists",
                HttpStatusCode.Conflict));
          else
            response = request.CreateResponse(
              OLabServerErrorResult.Result(
                $"Error creating static file '{phys.Path}'.  {ex.Message}",
                (HttpStatusCode)azureException.Status));
        }
        else
          response = request.CreateResponse(ex);

      }

      return response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("FileDelete")]
    public async Task<HttpResponseData> DeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "files/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        await _endpoint.DeleteAsync(auth, id);

        response = request.CreateResponse(new NoContentResult());

      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}