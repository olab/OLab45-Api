using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  public partial class MapsController : OlabController
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/nodes/{nodeId}/dynamicobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetDynamicScopedObjectsRawAsync(uint mapId, uint nodeId, [FromQuery] uint sinceTime = 0)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
        DynamicScopedObjectsDto dto = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/nodes/{nodeId}/dynamicobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetDynamicScopedObjectsTranslatedAsync(uint mapId, uint nodeId, [FromQuery] uint sinceTime = 0)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
        DynamicScopedObjectsDto dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Retrieve dynamic scoped objects for current node
    /// </summary>
    /// <param name="serverId">Server id</param>
    /// <param name="node">Current node</param>
    /// <param name="sinceTime">Look for changes since</param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    public async Task<IActionResult> GetDynamicScopedObjectsAsync(
      uint serverId,
      MapNodes node,
      uint sinceTime,
      bool enableWikiTranslation)
    {
      try
      {
        DynamicScopedObjectsDto dto = await _endpoint.GetDynamicScopedObjectsAsync(serverId, node, sinceTime, enableWikiTranslation); ;
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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