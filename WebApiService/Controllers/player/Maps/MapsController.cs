using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Services;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/maps")]
  [ApiController]
  public partial class MapsController : OlabController
  {
    private readonly MapsEndpoint _endpoint;

    public MapsController(ILogger<MapsController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new MapsEndpoint(this.logger, appSettings, context);
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
        Maps map = await _endpoint.GetSimpleAnonymousAsync(id);
        var dto = new MapsTestAccessDto
        {
          Id = map.Id,
          SecurityId = map.SecurityId
        };

        return Ok(dto);
      }
      catch (OLabObjectNotFoundException)
      {
        return OLabObjectResult<MapsTestAccessDto>.Result(new MapsTestAccessDto());
      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        OLabAPIPagedResponse<MapsDto> pagedResponse = await _endpoint.GetAsync(auth, take, skip);
        return OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMapPlayerAsync(uint id)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        MapsFullDto dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<MapsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        ExtendMapResponse dto = await _endpoint.PostExtendMapAsync(auth, mapId, body);
        return OLabObjectResult<ExtendMapResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        MapsFullRelationsDto dto = await _endpoint.PostCreateMapAsync(auth, body);
        return OLabObjectResult<MapsFullRelationsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        await _endpoint.PutAsync(auth, id, mapdto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        await _endpoint.DeleteAsync(auth, id);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        System.Collections.Generic.IList<MapNodeLinksFullDto> dtoList = await _endpoint.GetLinksAsync(auth, mapId);
        return OLabObjectListResult<MapNodeLinksFullDto>.Result(dtoList);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
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
}
