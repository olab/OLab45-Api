using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Endpoints;
using OLabWebAPI.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi;

[Route("olab/api/v3/import3")]
[ApiController]
public class Import3Controller : OLabController
{
  private readonly Import3Endpoint _endpoint;

  public Import3Controller(
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

    Logger = OLabLogger.CreateNew<Import3Controller>(loggerFactory, true);

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
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> Import(CancellationToken token)
  {
    Maps mapPhys = null;

    // validate token/setup up common properties
    var auth = GetAuthorization(HttpContext);

    if (Request.Body == null)
      throw new ArgumentNullException(nameof(Request.Body));

    using (var archiveFileStream = Request.Form.Files[0].OpenReadStream())
    {
      Logger.LogInformation($"Import archive file: {Request.Form.Files[0].FileName}. size {archiveFileStream.Length}");

      archiveFileStream.Position = 0;

      // test for bad file name (including any directory characters)
      if (Request.Form.Files[0].FileName.Contains(Path.DirectorySeparatorChar))
        Logger.LogError("Invalid file name");
      else
        mapPhys = await _endpoint.ImportAsync(
          auth,
          archiveFileStream,
          Request.Form.Files[0].FileName,
          token);
    }

    var dto = new ImportResponse
    {
      Id = mapPhys.Id,
      Name = mapPhys.Name,
      CreatedAt = mapPhys.CreatedAt.Value,
      LogMessages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info).Select(x => x.Message).ToList()
    };

    return HttpContext.Request.CreateResponse(OLabObjectResult<ImportResponse>.Result(dto));
  }

}
