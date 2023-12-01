using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/files")]
[ApiController]
public partial class FilesController : OLabController
{
  private readonly FilesEndpoint _endpoint;

  public FilesController(ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  OLabDBContext dbContext,
  IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
  IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
    configuration,
    dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<FilesController>(loggerFactory);

    _endpoint = new FilesEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  private async Task<IOLabFormFieldHelper> GetFormFieldHelperAsync(Stream stream, IFormCollection form)
  {
    var helper = new OLabFormFieldHelper(stream);

    Logger.LogInformation($"Form fields:");

    foreach (var field in form)
    {
      Logger.LogInformation($"  {field.Key} = {field.Value}");
      helper.Fields.Add(field.Key, field.Value);
    }

    if (form.Files.Count == 1)
    {
      var file = form.Files[0];
      await file.CopyToAsync(helper.Stream);
      helper.Stream.Position = 0;

      Logger.LogInformation($"  file: {file.FileName}. size {helper.Stream.Length}");
    }

    return helper;
  }

  /// <summary>
  /// Retrieve all files objects
  /// </summary>
  /// <param name="take">Pages max # records to retrieve</param>
  /// <param name="skip"># records to skip</param>
  /// <returns>Array of file records</returns>
  [HttpGet]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
  {
    try
    {
      Logger.LogDebug($"ReadAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var pagedResponse = await _endpoint.GetAsync(take, skip);
      return HttpContext.Request.CreateResponse(OLabObjectPagedListResult<FilesDto>.Result(pagedResponse.Data, pagedResponse.Remaining));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Create new file
  /// </summary>
  /// <param name="dto">File data</param>
  /// <returns>IActionResult</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostAsync(CancellationToken cancel)
  {
    try
    {
      Logger.LogDebug($"PostAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      using (var stream = new MemoryStream())
      {
        var formHelper = await GetFormFieldHelperAsync(stream, Request.Form);

        var dto = new FilesFullDto(formHelper);
        dto = await _endpoint.PostAsync(auth, dto, cancel);

        return HttpContext.Request.CreateResponse(OLabObjectResult<FilesFullDto>.Result(dto));
      }

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync(uint id)
  {
    try
    {
      Logger.LogDebug($"ReadAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<FilesFullDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Saves a file edit
  /// </summary>
  /// <param name="id">file id</param>
  /// <returns>IActionResult</returns>
  [HttpPut("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PutAsync(uint id, [FromBody] FilesFullDto dto)
  {
    try
    {
      Logger.LogDebug($"PutAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.PutAsync(auth, id, dto);
      return NoContent();
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpDelete("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> DeleteAsync(uint id)
  {
    try
    {
      Logger.LogDebug($"DeleteAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.DeleteAsync(auth, id);
      return NoContent();
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }
}
