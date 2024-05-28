using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
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
        .CreateResponse(OLabObjectPagedListResult<Roles>.Result(pagedResponse.Data, pagedResponse.Remaining));
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
  public async Task<IActionResult> PostAsync(string groupName, CancellationToken cancel)
  {
    try
    {
      Logger.LogDebug($"RolesEndpoint.PostAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);
      var phys = await _endpoint.PostAsync(auth, groupName, cancel);

      return HttpContext.Request.CreateResponse(OLabObjectResult<Roles>.Result(phys));
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
      Logger.LogDebug($"RolesEndpoint.GetAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var phys = await _endpoint.GetAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<Roles>.Result(phys));
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
      Logger.LogDebug($"RolesEndpoint.DeleteAsync");

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
