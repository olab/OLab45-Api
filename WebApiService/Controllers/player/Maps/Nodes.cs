using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;
using UserContext = OLabWebAPI.Data.UserContext;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    public partial class MapsController : OlabController
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
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                var userContext = new UserContext(logger, dbContext, HttpContext);
                _endpoint.SetUserContext(userContext);

                MapsNodesFullRelationsDto dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);

                if (body.IsEmpty() || (dto.TypeId == 1))
                    // requested a root node, so return an initial set of dynammic objects
                    dto.DynamicObjects = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId);
                else
                {
                    // apply any node open counter actions
                    System.Collections.Generic.IList<CountersDto> dtoList = await _endpoint.ProcessNodeOpenCountersAsync(nodeId, body.Map.Counters);
                    dto.DynamicObjects.Map.Counters.Clear();
                    dto.DynamicObjects.Map.Counters.AddRange(dtoList);
                }


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
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                MapNodesPostResponseDto dto = await _endpoint.DeleteNodeAsync(auth, mapId, nodeId);
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
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                MapNodesPostResponseDto newDto = await _endpoint.PutNodeAsync(auth, mapId, nodeId, dto);
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
