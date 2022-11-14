using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OLab.Endpoints.Azure.Player
{
  public partial class MapsFunction : OLabFunction
  {
    /// <summary>
    /// Plays specific map node
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <returns>IActionResult</returns>
    [FunctionName("MapNodeGet")]
    public async Task<IActionResult> MapNodeGetAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{mapId}/node/{nodeId}")] HttpRequest request,
      uint mapId, 
      uint nodeId)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);

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
    [FunctionName("MapNodeDelete")]
    public async Task<IActionResult> MapNodeDeleteAsync(
      [HttpTrigger(AuthorizationLevel.User, "delete", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

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
    [FunctionName("MapNodePut")]
    public async Task<IActionResult> MapNodePutAsync(
      [HttpTrigger(AuthorizationLevel.User, "put", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        MapNodesFullDto body = JsonConvert.DeserializeObject<MapNodesFullDto>(content);

        var newDto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, body);
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
