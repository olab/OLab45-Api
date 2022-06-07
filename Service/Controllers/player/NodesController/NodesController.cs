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

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/nodes")]
  [ApiController]
  public partial class NodesController : OlabController
  {
    public NodesController(ILogger<NodesController> logger, OLabDBContext context) : base(logger, context)
    {
    }

    /// <summary>
    /// Get simple map node, no relations
    /// </summary>
    /// <param name="context">EF DBContext</param>
    /// <param name="id">Node Id</param>
    /// <returns>MapNodes</returns>
    private static Model.MapNodes GetSimple(OLabDBContext context, uint id)
    {
      var phys = context.MapNodes.FirstOrDefault(x => x.Id == id);
      return phys;
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
      return await GetNodeAsync(nodeId, true);
    }

    private async Task<IActionResult> GetNodeAsync(uint id, bool enableWikiTranslation)
    {
      logger.LogDebug($"NodesController.GetNodeAsync(uint nodeId={id}, bool enableWikiTranslation={enableWikiTranslation})");

      var phys = await GetMapNodeAsync(id);

      if (phys.Id == 0)
        return OLabNotFoundResult<uint>.Result(id);

      var builder = new ObjectMapper.MapsNodesFullRelationsMapper(logger, enableWikiTranslation);
      var dto = builder.PhysicalToDto(phys);

      return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
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
      var phys = await GetMapNodeAsync(id);
      if (phys == null)
        return OLabNotFoundResult<uint>.Result(id);

      var builder = new ObjectMapper.MapNodesFullMapper(logger);
      phys = builder.DtoToPhysical(dto);

      context.Entry(phys).State = EntityState.Modified;

      await context.SaveChangesAsync();

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
      logger.LogDebug($"MapsNodesController.PostAsync(PostLinkAsync(uint mapId = {mapId}, uint nodeId = {nodeId}, [FromBody] uint destinationId = {data.DestinationId})");

      MapNodeLinks phys = MapNodeLinks.CreateDefault();
      phys.NodeId1 = nodeId;
      phys.NodeId2 = data.DestinationId;
      phys.MapId = mapId;

      context.MapNodeLinks.Add(phys);
      await context.SaveChangesAsync();
      logger.LogDebug($"created MapNodeLink id = {phys.Id}");

      var dto = new MapNodeLinksPostResponseDto
      {
        Id = phys.Id
      };

      return OLabObjectResult<MapNodeLinksPostResponseDto>.Result(dto);
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
      logger.LogDebug($"MapsNodesController.PostAsync(MapNodesFullDto dtoNode)");

      using var transaction = context.Database.BeginTransaction();

      try
      {
        Model.MapNodes phys = Model.MapNodes.CreateDefault();
        phys.X = data.X;
        phys.Y = data.Y;
        phys.MapId = mapId;

        context.MapNodes.Add(phys);
        await context.SaveChangesAsync();
        logger.LogDebug($"created MapNode id = {phys.Id}");

        MapNodeLinks link = new MapNodeLinks
        {
          MapId = mapId,
          NodeId1 = data.SourceId,
          NodeId2 = phys.Id
        };

        context.MapNodeLinks.Add(link);
        await context.SaveChangesAsync();
        logger.LogDebug($"created MapNodeLink id = {link.Id}");

        await transaction.CommitAsync();

        var dto = new MapNodesPostResponseDto
        {
          Id = phys.Id
        };
        dto.Links.Id = link.Id;

        return OLabObjectResult<MapNodesPostResponseDto>.Result(dto);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        throw ex;
      }

    }
  }
}
