using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Importer;
using OLab.Api.Model;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using OLab.Import.Interfaces;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using Dawn;
using OLab.Endpoints;
using OLab.Api.Common.Exceptions;
using HttpMultipartParser;

namespace OLabWebAPI.Endpoints.WebApi
{
  [Route("olab/api/v3/[controller]/[action]")]
  [ApiController]
  public class ImportController : OLabController
  {
    private readonly ImportEndpoint _endpoint;

    public ImportController(
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

      Logger = OLabLogger.CreateNew<ImportController>(loggerFactory);

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
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Post()
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      if (!auth.HasAccess("X", "Import", 0))
        throw new OLabUnauthorizedException();

      if (Request.Body == null)
        throw new ArgumentNullException(nameof(Request.Body));

      var parser = await MultipartFormDataParser.ParseAsync(Request.Body);
      var file = parser.Files[0];

      // test for bad file name (including any directory characters)
      if (file.FileName.Contains(Path.DirectorySeparatorChar))
        Logger.LogError("Invalid file name");
      else
      {
        //  var fullFileName = Path.Combine(GetUploadDirectory(), file.FileName);

        //  if (!File.Exists(fullFileName))
        //    _logger.LogError("Unable to load file");
        //  else
        //  {
        //    _logger.LogInformation($"Loading archive: '{Path.GetFileName(fullFileName)}'");

        //    if (_importer.ProcessImportFileAsync(fullFileName))
        //      _importer.WriteImportToDatabase();
        //  }
      }

      var dto = new ImportResponse
      {
        Messages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info)
      };

      return OLabObjectResult<ImportResponse>.Result(dto);
    }

  }

}
