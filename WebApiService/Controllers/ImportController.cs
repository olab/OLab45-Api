using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLab.Endpoints;
using OLab.Import;
using OLabWebAPI.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi;

[Route("olab/api/v3/import")]
[ApiController]
public class ImportController : OLabController
{
  private readonly Import4Endpoint _endpoint4;
  private readonly Import3Endpoint _endpoint3;

  public ImportController(
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

    Logger = OLabLogger.CreateNew<ImportController>(loggerFactory, true);

    _endpoint4 = new Import4Endpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);

    _endpoint3 = new Import3Endpoint(
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
  public async Task<IActionResult> Import(
    CancellationToken token)
  {
    try
    {
      Maps mapPhys = null;

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      if (Request.Form == null)
        throw new ArgumentNullException(nameof(Request.Form));

      var form = Request.Form;

      if (form.Files[0].FileName.Contains(Path.DirectorySeparatorChar))
        throw new Exception("Invalid file name");

      using (var stream = new MemoryStream())
      {
        var helper = new OLabFormFieldHelper(stream);

        var file = Request.Form.Files[0];
        await file.CopyToAsync(helper.Stream);

        helper.Stream.Position = 0;

        Logger.LogInformation($"Import archive file: {Request.Form.Files[0].FileName}. size {stream.Length}");

        // get list of archive files to determine if OLab3/4 import
        var files = ZipFileHelper.GetFiles(stream);

        if (files.Contains("map.json"))
          mapPhys = await _endpoint4.ImportAsync(
            auth,
            stream,
            Request.Form.Files[0].FileName,
            token);

        if (files.Contains("map.xml"))
          mapPhys = await _endpoint3.ImportAsync(
            auth,
            stream,
            Request.Form.Files[0].FileName,
            token);
      }

      var dto = new ImportResponse
      {
        Id = mapPhys.Id,
        Name = mapPhys.Name,
        CreatedAt = mapPhys.CreatedAt.Value,
        LogMessages = Logger.GetMessages(OLabLogMessage.MessageLevel.Info).Select(x => x.Message).ToList(),
        //Groups = MapGroups.ToString(mapPhys.MapGroups.ToList())
      };

      return HttpContext.Request.CreateResponse(
        OLabObjectResult<ImportResponse>.Result(dto));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  [HttpGet("export/{id}/json")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> ExportAsJson(
    uint id,
    CancellationToken token)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      using var memoryStream = new MemoryStream();
      var dto = await _endpoint4.ExportAsync(id, token);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsFullRelationsDto>.Result(dto));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  [HttpGet("export/{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> Export(
    uint id,
    CancellationToken token)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      using var memoryStream = new MemoryStream();
      await _endpoint4.ExportAsync(
        memoryStream,
        id, 
        token);

      memoryStream.Position = 0;
      var now = DateTime.UtcNow;
      return File(
        memoryStream.ToArray(),
        "application/zip",
        $"OLab4Export.map{id}.{now.ToString("yyyyMMddHHmm")}.zip");

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

}