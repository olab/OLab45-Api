using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Dto;

using OLab.FunctionApp.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.Player;

public partial class MapsFunction : OLabFunction
{
  /// <summary>
  /// Plays specific map node
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <returns>IActionResult</returns>
  [Function("PostNode")]
  public async Task<HttpResponseData> PostMapNodeAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "maps/{mapId}/node/{nodeId}")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId)
  {

    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var body = await request.ParseBodyFromRequestAsync<DynamicScopedObjectsDto>();
      var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId, body);

      response = request.CreateResponse(OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

  /// <summary>
  /// Delete a node from the map
  /// </summary>
  /// <param name="mapId">map id that owns node</param>
  /// <param name="nodeId">node id</param>
  /// <returns>IActionResult</returns>
  [Function("DeleteNode")]
  public async Task<HttpResponseData> DeleteNodeAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.DeleteNodeAsync(auth, mapId, nodeId);
      response = request.CreateResponse(OLabObjectResult<MapNodesPostResponseDto>.Result(dto));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

  /// <summary>
  /// Updates specific map node
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <param name="dto">node data</param>
  /// <returns>IActionResult</returns>
  [Function("PutNode")]
  public async Task<HttpResponseData> PutNodeAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var body = await request.ParseBodyFromRequestAsync<MapNodesFullDto>();
      var dto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, body);

      response = request.CreateResponse(OLabObjectResult<MapNodesPostResponseDto>.Result(dto));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;
  }

}
