using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.StaticFiles;
using Azure.Storage.Blobs;

namespace OLab.Endpoints.Azure
{
  public class FilesAzureEndpoint : OLabAzureEndpoint
  {
    private readonly FilesEndpoint _endpoint;
    private readonly AppSettings _appSettings;

    public FilesAzureEndpoint(
      IUserService userService,
      IOptions<AppSettings> appSettings,
      ILogger<CountersAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(appSettings).NotNull(nameof(appSettings));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _appSettings = appSettings.Value;
      _endpoint = new FilesEndpoint(this.logger, context);
    }

    public string GetMimeTypeForFileExtension(string filePath)
    {
      const string DefaultContentType = "application/octet-stream";

      var provider = new FileExtensionContentTypeProvider();

      if (!provider.TryGetContentType(filePath, out string contentType))
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

      logger.LogDebug($"Static file name: {tempFileName}");

      return tempFileName;
    }

    /// <summary>
    /// Gets all counters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("FilesGet")]
    public async Task<IActionResult> FilesGetAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "files")] HttpRequest request,
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
        AuthorizeRequest(request);

        var pagedResult = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<FilesDto>.Result(pagedResult.Data, pagedResult.Remaining);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);

        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Gets single constant
    /// </summary>
    /// <param name="id">Counter id</param>
    /// <returns></returns>
    [FunctionName("FileGet")]
    public async Task<IActionResult> FileGetAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "files/{id}")] HttpRequest request,
      uint id
    )
    {
      Guard.Argument(request).NotNull(nameof(request));

      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        var dto = await _endpoint.GetAsync(auth, id);
        var blobName = BuildStaticFileName(dto);

        // generate short-lived blob download url
        var sasGenerator = new AzureStorageBlobOptionsTokenGenerator(_appSettings);
        dto.Url = sasGenerator.GenerateSasToken(_appSettings.StaticFilesContainerName, blobName);

        return OLabObjectResult<FilesFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Create new file
    /// </summary>
    /// <param name="dto">File data</param>
    /// <returns>IActionResult</returns>
    [FunctionName("FilePost")]
    public async Task<IActionResult> FilePostAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "files")] HttpRequest request
    )
    {
      SystemFiles phys = null;

      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        logger.LogDebug($"FilesController.PostAsync()");
        var dto = new FilesFullDto(request.Form);

        var builder = new FilesFull(logger);
        phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        string staticFileName = BuildStaticFileName(dto);

        // save just the file name to the database
        phys.Path = Path.GetFileName(staticFileName);
        // infer the mimetype from the file name
        phys.Mime = GetMimeTypeForFileExtension(phys.Name);
        dto = await _endpoint.PostAsync(auth, phys);

        // TODO: successful save to database, copy the file to the
        // final location
        Stream myBlob = new MemoryStream();
        var file = request.Form.Files["selectedFile"];
        myBlob = file.OpenReadStream();
        var blobClient = new BlobContainerClient(_appSettings.StaticFilesConnectionString, _appSettings.StaticFilesContainerName);
        var blob = blobClient.GetBlobClient(staticFileName);
        await blob.UploadAsync(myBlob);

        return OLabObjectResult<FilesFullDto>.Result(dto);

      }
      catch (Exception ex)
      {
        if ( ex is global::Azure.RequestFailedException )
        {
          var azureException = ex as global::Azure.RequestFailedException;
          if ( azureException.Status == 409 )
            return OLabServerErrorResult.Result($"File '{phys.Path}' already exists", HttpStatusCode.Conflict);
          return OLabServerErrorResult.Result($"Error creating static file '{phys.Path}'.  {ex.Message}", ( HttpStatusCode )azureException.Status);
        }

        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [FunctionName("FileDelete")]
    public async Task<IActionResult> DeleteAsync(
      [HttpTrigger(AuthorizationLevel.User, "delete", Route = "files/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);
        
        await _endpoint.DeleteAsync(auth, id);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return new NoContentResult();
    }

  }
}
