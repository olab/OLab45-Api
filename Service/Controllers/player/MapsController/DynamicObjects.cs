using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;

namespace OLabWebAPI.Controllers.Player
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
        var dto = await _endpoint.GetDynamicScopedObjectsRawAsync(mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
      }
      catch (OLabUnauthorizedException ex)
      {
        return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
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
        var dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
      }
      catch (OLabUnauthorizedException ex)
      {
        return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
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
      Model.MapNodes node,
      uint sinceTime,
      bool enableWikiTranslation)
    {
      try
      {
        var dto = await _endpoint.GetDynamicScopedObjectsAsync(serverId, node, sinceTime, enableWikiTranslation);;
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
      }
      catch (OLabUnauthorizedException ex)
      {
        return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
      }
    }

  }
}