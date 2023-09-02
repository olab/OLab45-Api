using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using OLab.FunctionApp.Api;
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
    [FunctionName("PostNode")]
    public async Task<IActionResult> PostMapNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "maps/{mapId}/node/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

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
    [FunctionName("DeleteNode")]
    public async Task<IActionResult> DeleteNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

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
    [FunctionName("PutNode")]
    public async Task<IActionResult> PutNodeAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

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
