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
      IUserService userService,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
        configuration,
        userService,
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

    private string GetUploadDirectory()
    {
      return _configuration.GetAppSettings().ImportFolder;
    }

    [Function("Upload")]
    public async Task<IActionResult> UploadAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import/upload")] HttpRequestData request,
        FunctionContext hostContext,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));

      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        // test if user has access to import.
        var userContext = auth.GetUserContext();
        if (!userContext.HasAccess("X", "Import", 0))
          throw new OLabUnauthorizedException();

        if (request.Body == null)
          throw new ArgumentNullException(nameof(request.Body));

        var httpContext = request.AsHttpContext();
        var files = httpContext.Request.Form.Files.ToList();

        if ( files == null || files.Count == 0)
          throw new Exception("unable to get request file");

        var file = files.FirstOrDefault();

        await _endpoint.Import(file, cancellationToken);      
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      var dto = new ImportResponse
      {
        Messages = Logger.GetMessages()
      };

      return OLabObjectResult<ImportResponse>.Result(dto);
    }

    /// <summary>
    /// Runs an import
    /// </summary>
    /// <param name="request">ImportRequest</param>
    /// <returns>IActionResult</returns>
    [Function("Import")]
    public async Task<HttpResponseData> ImportAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import")] HttpRequestData request,
      FunctionContext hostContext)
    {
      Logger.LogDebug($"FilePostAsync");

      // validate token/setup up common properties
      var auth = GetRequestContext(hostContext);

      // test if user has access to map.
      var userContext = auth.GetUserContext();
      if (!userContext.HasAccess("X", "Import", 0))
        throw new OLabUnauthorizedException();

      if (request.Body == null)
        throw new ArgumentNullException(nameof(request.Body));

      var parser = await MultipartFormDataParser.ParseAsync(request.Body);
      Stream myBlob = new MemoryStream();
      var file = parser.Files[0];

      // test for bad file name (including any directory characters)
      if (file.FileName.Contains(Path.DirectorySeparatorChar))
        Logger.LogError("Invalid file name");
      else
      {
        //  var fullFileName = Path.Combine(GetUploadDirectory(), file.FileName);

        //  if (!File.Exists(fullFileName))
        //    Logger.LogError("Unable to load file");
        //  else
        //  {
        //    Logger.LogInformation($"Loading archive: '{Path.GetFileName(fullFileName)}'");

        //    if (_importer.ProcessImportFileAsync(fullFileName))
        //      _importer.WriteImportToDatabase();
        //  }
      }

      var dto = new ImportResponse
      {
        Messages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info)
      };
      response = request.CreateResponse(OLabObjectResult<ImportResponse>.Result(dto));
      return response;
    }

    private bool CheckIfValidFile(string path)
    {
      var rc = true;

      try
      {
        using (var zipFile = ZipFile.OpenRead(path))
        {
          var entries = zipFile.Entries;
        }
      }
      catch (InvalidDataException)
      {
        rc = false;
      }

      Logger.LogInformation($"Export file '{path}' valid? {rc}");

      return rc;
    }

    //private string WriteFile(IFormFile file)
    //{
    //  // strip off any directory
    //  var fileName = Path.GetRandomFileName();
    //  fileName += Path.GetExtension(file.FileName);

    //  using (var stream = new FileStream(path, FileMode.Create))
    //  {
    //    await file.CopyToAsync(stream);
    //    Logger.LogInformation($"Wrote upload file to '{path}'. Size: {file.Length}");
    //  }

    //  return path;
    //}

  }

}
