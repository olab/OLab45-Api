using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

public partial class MapsController : OLabController
{
  /// <summary>
  /// Plays specific map node with a dynamic object state
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <returns>IActionResult</returns>
  [HttpPost("{mapId}/node/{nodeId}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostMapNodeAsync(
    uint mapId,
    uint nodeId,
    [FromBody] DynamicScopedObjectsDto body)
  {

    try
    {
      Guard.Argument(mapId, nameof(mapId)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);
      var dto = await _endpoint.PostMapNodeAsync(auth, mapId, nodeId, body);

      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.DeleteNodeAsync(auth, mapId, nodeId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapNodesPostResponseDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var newDto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, dto);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapNodesPostResponseDto>.Result(newDto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

}
