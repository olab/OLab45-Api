using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class MapsFunction : OLabFunction
  {
    /// <summary>
    /// Plays specific map node
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <returns>IActionResult</returns>
    [Function("PostNode")]
    public async Task<IActionResult> PostMapNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "maps/{mapId}/node/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId,
      uint nodeId)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);

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
    [Function("DeleteNode")]
    public async Task<IActionResult> DeleteNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
    [Function("PutNode")]
    public async Task<IActionResult> PutNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<MapNodesFullDto>();
        var dto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, body);

        return OLabObjectResult<MapNodesPostResponseDto>.Result(dto);
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
