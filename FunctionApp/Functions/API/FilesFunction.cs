using Dawn;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.ObjectMapper;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;
using System.Net;

namespace OLab.FunctionApp.Functions.API
{
    public class FilesFunction : OLabFunction
  {
    private readonly FilesEndpoint _endpoint;

    public FilesFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(configuration, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<FilesFunction>(loggerFactory);
      _endpoint = new FilesEndpoint(Logger, configuration, dbContext, wikiTagProvider, fileStorageProvider);
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

    private IOLabFormFieldHelper GetFormFieldHelperAsync(Stream stream, MultipartFormDataParser parser)
    {
      var helper = new OLabFormFieldHelper(stream);

      helper.Fields.Add("id", Convert.ToUInt32(parser.GetParameterValue("id")));
      helper.Fields.Add("name", parser.GetParameterValue("name"));
      helper.Fields.Add("description", parser.GetParameterValue("description"));
      helper.Fields.Add("copyright", parser.GetParameterValue("copyright"));
      helper.Fields.Add("parentId", Convert.ToUInt32(parser.GetParameterValue("parentId")));
      helper.Fields.Add("scopeLevel", parser.GetParameterValue("scopeLevel"));
      helper.Fields.Add("isMediaResource", Convert.ToBoolean(parser.GetParameterValue("isMediaResource")));
      helper.Fields.Add("selectedFileName", parser.GetParameterValue("selectedFileName"));
      helper.Fields.Add("fileSize", Convert.ToInt32(parser.GetParameterValue("fileSize")));

      Logger.LogInformation($"Form fields:");

      foreach (var field in helper.Fields)
        Logger.LogInformation($"  {field.Key} = {field.Value}");

      helper.Stream = parser.Files[0].Data;
      helper.Stream.Position = 0;

      if (helper.Stream.Length > 0)
        Logger.LogInformation($"  file: {helper.Field("selectedFileName")}. size {helper.Stream.Length}");

      return helper;
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
        Logger.LogDebug($"FilesGet");

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

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
        Logger.LogDebug($"FileGet");

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

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
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "files")] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken token)
    {
      string fileName = "";

      try
      {
        Logger.LogDebug($"FilePost");

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var parser = await MultipartFormDataParser.ParseAsync(request.Body).ConfigureAwait(false);

        using (var stream = new MemoryStream())
        {
          var formHelper = GetFormFieldHelperAsync(stream, parser);

          var dto = new FilesFullDto(formHelper);
          dto = await _endpoint.PostAsync(auth, dto, token);

          response = request.CreateResponse(OLabObjectResult<FilesFullDto>.Result(dto));
        }

      }
      catch (Exception ex)
      {
        if (ex is Azure.RequestFailedException)
        {
          var azureException = ex as Azure.RequestFailedException;
          if (azureException.Status == 409)
            response = request.CreateResponse(
              OLabServerErrorResult.Result(
                $"File '{fileName}' already exists",
                HttpStatusCode.Conflict));
          else
            response = request.CreateResponse(
              OLabServerErrorResult.Result(
                $"Error creating static file '{fileName}'.  {ex.Message}",
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
        Logger.LogDebug($"FileDelete");

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

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
