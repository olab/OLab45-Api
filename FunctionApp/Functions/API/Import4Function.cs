using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Api.Dto;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.Endpoints;
using OLab.FunctionApp.Extensions;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace OLab.FunctionApp.Functions.API;

public class Import4Function : OLabFunction
{
  private readonly Import4Endpoint _endpoint;

  public Import4Function(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<Import4Function>(loggerFactory, true);

    _endpoint = new Import4Endpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Runs an import
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>IActionResult</returns>
  [Function("Import4")]
  public async Task<HttpResponseData> ImportAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import4")] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel)
  {
    try
    {
      Logger.LogDebug($"ImportAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      if (!auth.HasAccess("X", "Import", 0))
        throw new OLabUnauthorizedException();

      if (request.Body == null)
        throw new ArgumentNullException(nameof(request.Body));

      var parser = await MultipartFormDataParser.ParseAsync(request.Body);
      if (parser.Files.Count == 0)
        throw new Exception("No files were uploaded");

      var stream = parser.Files[0].Data;

      Logger.LogInformation($"Loading archive: '{parser.Files[0].FileName}'");

      var mapId = await _endpoint.ImportAsync(stream, parser.Files[0].FileName, cancel);

      var dto = new ImportResponse
      {
        MapId = mapId,
        LogMessages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info).Select(x => x.Message).ToList()
      };

      response = request.CreateResponse(OLabObjectResult<ImportResponse>.Result(dto));

      return response;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

  [Function("Export4AsJson")]
  public async Task<HttpResponseData> ExportAsJsonAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "import4/export/{id}/json")] HttpRequestData request,
    FunctionContext hostContext,
    uint id,
    CancellationToken token)
  {
    try
    {
      Logger.LogDebug($"ExportAsJsonAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      if (!auth.HasAccess("X", "Export", 0))
        throw new OLabUnauthorizedException();

      var dto = await _endpoint.ExportAsync(id, token);
      response = request.CreateResponse(OLabObjectResult<MapsFullRelationsDto>.Result(dto));

    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

  [Function("Export4")]
  public async Task<HttpResponseData> ExportAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "import4/export/{id}")] HttpRequestData request,
    FunctionContext hostContext,
    uint id,
    CancellationToken token)
  {
    try
    {
      Logger.LogDebug($"Export");

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      if (!auth.HasAccess("X", "Export", 0))
        throw new OLabUnauthorizedException();

      using (var memoryStream = new MemoryStream())
      {
        await _endpoint.ExportAsync(memoryStream, id, token);

        memoryStream.Position = 0;
        var now = DateTime.UtcNow;

        var fileDownloadName = $"OLab4Export.map{id}.{now.ToString("yyyyMMddHHmm")}.zip";

        response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteBytes(memoryStream.ToArray());
        response.Headers.Add("Content-Type", "application/zip");
        response.Headers.Add("Content-Length", $"{memoryStream.Length}");
        response.Headers.Add("Content-Disposition", $"attachment; filename={fileDownloadName}; filename*=UTF-8'{fileDownloadName}");
      }

    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }
}

