using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;
using System;
using OLabWebAPI.Common;
using OLabWebAPI.Endpoints.Player;
using OLabWebAPI.Common.Exceptions;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/nodes")]
  [ApiController]
  public partial class NodesController : OlabController
  {
    private readonly NodesEndpoint _endpoint;

    public NodesController(ILogger<NodesController> logger, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new NodesEndpoint(this.logger, context);
    }

    /// <summary>
    /// Get full map node, with relations
    /// </summary>
    /// <param name="nodeId">Node id (0, if root node)</param>
    /// <returns>MapsNodesFullRelationsDto response</returns>
    [HttpGet("{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetNodeTranslatedAsync(uint nodeId)
    {
      try
      {
        var dto = await _endpoint.GetNodeTranslatedAsync(nodeId);
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
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutNodeAsync(uint id, [FromBody] MapNodesFullDto dto)
    {
      try
      {
        await _endpoint.PutNodeAsync(id, dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("{nodeId}/links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostLinkAsync(
      uint mapId,
      uint nodeId,
      [FromBody] MapNodeLinksPostDataDto data
    )
    {
      try
      {
        var dto = await _endpoint.PostLinkAsync(mapId, nodeId, data);
        return OLabObjectResult<MapNodeLinksPostResponseDto>.Result(dto);
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
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("{nodeId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostNodeAsync(
      uint mapId,
      [FromBody] MapNodesPostDataDto data
    )
    {
      try
      {
        var dto = await _endpoint.PostNodeAsync(mapId, data);
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
