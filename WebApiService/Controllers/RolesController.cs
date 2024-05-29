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
using OLab.Data.Interface;
using OLab.Data.Mappers;
using OLabWebAPI.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/roles")]
[ApiController]
public partial class RolesController : OLabController
{
  private readonly RolesEndpoint _endpoint;

  public RolesController(ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  OLabDBContext dbContext,
  IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
  IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
    configuration,
    dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<RolesController>(loggerFactory);

    _endpoint = new RolesEndpoint(
      Logger,
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Get single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{source}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync(string source)
  {
    try
    {
      Logger.LogDebug($"RolesEndpoint.GetAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetAsync(auth, source);

      return HttpContext.Request.CreateResponse(OLabObjectResult<RolesDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Retrieve all group objects
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
      Logger.LogDebug($"RolesEndpoint.GetAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var pagedResponse = await _endpoint.GetAsync(auth, take, skip);
      return HttpContext.Request
        .CreateResponse(OLabObjectPagedListResult<RolesDto>.Result(pagedResponse.Data, pagedResponse.Remaining));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Create new group
  /// </summary>
  /// <param name="name">Name to create</param>
  /// <returns>IActionResult</returns>
  [HttpPost("{name}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostAsync(string name, CancellationToken cancel)
  {
    try
    {
      Logger.LogDebug($"RolesEndpoint.PostAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);
      var dto = await _endpoint.PostAsync(auth, name, cancel);

      return HttpContext.Request.CreateResponse(OLabObjectResult<RolesDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Delete object
  /// </summary>
  /// <param name="source">Name or id to delete</param>
  /// <returns></returns>
  [HttpDelete("{source}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> DeleteAsync(string source)
  {
    try
    {
      Logger.LogDebug($"RolesEndpoint.DeleteAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.DeleteAsync(auth, source);
      return NoContent();
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }
}
