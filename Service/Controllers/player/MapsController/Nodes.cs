using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using OLabWebAPI.Model.ReaderWriter;
using System;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Services;

namespace OLabWebAPI.Controllers.Player
{
  public partial class MapsController : OlabController
  {
    /// <summary>
    /// Plays specific map node
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <returns>IActionResult</returns>
    [HttpGet("{mapId}/node/{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMapNodeAsync(uint mapId, uint nodeId)
    {

      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);

        var userContext = new UserContext(logger, context, HttpContext);

        // test if end node, meaning we can close the session.  otherwise
        // record the OnPlay event
        if (dto.End.HasValue && dto.End.Value)
        {
          userContext.Session.OnPlayNode(userContext.SessionId, mapId, dto.Id.Value);
          userContext.Session.OnEndSession(userContext.SessionId, mapId, dto.Id.Value);
        }
        else
        {
          if (nodeId == 0)
          {
            userContext.Session.OnStartSession(userContext.UserName, mapId, userContext.IPAddress);
            dto.SessionId = userContext.Session.GetSessionId();
            userContext.SessionId = dto.SessionId;
          }

          userContext.Session.OnPlayNode(userContext.SessionId, mapId, dto.Id.Value);
        }

        _endpoint.UpdateNodeCounter();

        return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Delete a node from the map
    /// </summary>
    /// <param name="mapId">map id that owns node</param>
    /// <param name="nodeId">node id</param>
    /// <returns>IActionResult</returns>
    [HttpDelete("{mapId}/nodes/{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteNodeAsync(
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.DeleteNodeAsync(auth, mapId, nodeId);
        return OLabObjectResult<MapNodesPostResponseDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Updates specific map node
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <param name="dto">node data</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{mapId}/nodes/{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutNodeAsync(
      uint mapId,
      uint nodeId,
      [FromBody] MapNodesFullDto dto
    )
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var newDto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, dto);
        return OLabObjectResult<MapNodesPostResponseDto>.Result(newDto);
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
