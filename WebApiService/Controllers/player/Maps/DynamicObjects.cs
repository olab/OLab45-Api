using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  public partial class MapsController : OLabController
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
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        var dto = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId, sinceTime);
        return HttpContext.Request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
        return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
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
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        var dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(auth, mapId, nodeId, sinceTime);
        return HttpContext.Request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
        return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
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
        Guard.Argument(serverId, nameof(serverId)).NotZero();
        Guard.Argument(node).NotNull(nameof(node));

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        var dto = await _endpoint.GetDynamicScopedObjectsAsync(serverId, node, sinceTime, enableWikiTranslation); ;
        return HttpContext.Request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
        return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
      }
    }

  }
}