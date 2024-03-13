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

public class Import3Function : OLabFunction
{
  private readonly Import3Endpoint _endpoint;

  public Import3Function(
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

    _endpoint = new Import3Endpoint(
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
  [Function("Import3")]
  public async Task<HttpResponseData> ImportAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import3")] HttpRequestData request,
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

      var mapPhys = await _endpoint.ImportAsync(
        auth, 
        stream, 
        parser.Files[0].FileName, 
        cancel);

      var dto = new ImportResponse
      {
        Id = mapPhys.Id,
        Name = mapPhys.Name,
        CreatedAt = mapPhys.CreatedAt.Value,
        LogMessages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info).Select(x => x.Message).ToList()
      };

      var result = OLabObjectResult<ImportResponse>.Result(dto);
      result.Message = Logger.HasErrorMessage() ? "error" : "success";
      response = request.CreateResponse(result);
      
      return response;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

}

