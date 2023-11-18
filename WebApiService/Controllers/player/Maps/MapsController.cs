using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/maps")]
[ApiController]
public partial class MapsController : OLabController
{
  private readonly MapsEndpoint _endpoint;

  public MapsController(
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

    Logger = OLabLogger.CreateNew<MapsController>(loggerFactory);

    _endpoint = new MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Get security level of map
  /// </summary>
  /// <param name="id">Map Id to retrieve</param>
  /// <returns>MapsTestAccessDto</returns>
  [AllowAnonymous]
  [HttpGet("accesstype/{id}")]
  public async Task<IActionResult> GetMapAccessTypeAsync(uint id)
  {
    try
    {
      var map = await _endpoint.GetSimpleAnonymousAsync(id);
      var dto = new MapsTestAccessDto
      {
        Id = map.Id,
        SecurityId = map.SecurityId
      };

      return Ok(dto);
    }
    catch (OLabObjectNotFoundException)
    {
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsTestAccessDto>.Result(new MapsTestAccessDto()));
    }
    catch (Exception ex)
    {
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }

    /// <summary>
    /// Retrieve all sessions for a given map
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/sessions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetSessionsAsync(uint mapId)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
        System.Collections.Generic.IList<SessionInfo> dtoList = await _endpoint.GetSessionsAsync(auth, mapId);
        return OLabObjectListResult<SessionInfo>.Result(dtoList);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }
  }

  /// <summary>
  /// Get a list of maps
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [HttpGet]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetMapsPlayerAsync([FromQuery] int? take, [FromQuery] int? skip)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var pagedResponse = await _endpoint.GetAsync(auth, take, skip);
      return HttpContext.Request.CreateResponse(OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining));
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
  [AllowAnonymous]
  //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetMapPlayerAsync(uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsFullDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Append template to an existing map
  /// </summary>
  /// <param name="mapId">Map to add template to</param>
  /// <param name="CreateMapRequest.templateId">Template to add to map</param>
  /// <returns>IActionResult</returns>
  [HttpPost("{mapId}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostAppendTemplateToMapPlayerAsync([FromRoute] uint mapId, [FromBody] ExtendMapRequest body)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.PostExtendMapAsync(auth, mapId, body);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ExtendMapResponse>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Create new map (using optional template)
  /// </summary>
  /// <param name="body">Create map request body</param>
  /// <returns>IActionResult</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostCreateMapAsync([FromBody] CreateMapRequest body)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.PostCreateMapAsync(auth, body);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsFullRelationsDto>.Result(dto));
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
  /// <param name="mapdto"></param>
  /// <returns></returns>
  [HttpPut("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PutAsync(uint id, MapsFullDto mapdto)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.PutAsync(auth, id, mapdto);
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

    return NoContent();
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.DeleteAsync(auth, id);
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

    return NoContent();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <returns></returns>
  [HttpGet("{mapId}/links")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetLinksAsync(uint mapId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dtoList = await _endpoint.GetLinksAsync(auth, mapId);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<MapNodeLinksFullDto>.Result(dtoList));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  [HttpOptions]
  public void Options()
  {

  }

  /// <summary>
  /// Retrieve all sessions for a given map
  /// </summary>
  /// <param name="mapId"></param>
  /// <returns></returns>
  [HttpGet("{mapId}/sessions")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetSessionsAsync(uint mapId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dtoList = await _endpoint.GetSessionsAsync(auth, mapId);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<SessionInfo>.Result(dtoList));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }
}
