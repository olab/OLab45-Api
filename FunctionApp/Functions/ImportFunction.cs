using Azure.Storage.Blobs;
using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Importer;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data;
using OLab.Data.Interface;
using OLab.Endpoints;
using OLab.FunctionApp.Extensions;
using OLab.Import.Interfaces;
using SharpCompress.Compressors.Xz;
using System.IO;
using System.IO.Compression;

namespace OLab.FunctionApp.Functions
{
  public class ImportFunction : OLabFunction
  {
    private readonly ImportEndpoint _endpoint;

    public ImportFunction(
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

      Logger = OLabLogger.CreateNew<ImportFunction>(loggerFactory);

      _endpoint = new ImportEndpoint(
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
    [Function("Import")]
    public async Task<HttpResponseData> ImportAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import")] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken cancel)
    {
      Logger.LogDebug($"FilePostAsync");

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

      _endpoint.Import(stream, cancel);

      var dto = new ImportResponse
      {
        Messages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info)
      };
      response = request.CreateResponse(OLabObjectResult<ImportResponse>.Result(dto));
      return response;

    }
  }
}

