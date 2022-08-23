using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using System.Text;
using OLabWebAPI.Model;
using OLabWebAPI.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OLabWebAPI.Controllers.Player;
using System.IO;
using OLabWebAPI.Model.ReaderWriter;
using System.Collections.Generic;

namespace OLabWebAPI.Controllers.Designer
{
  [Route("olab/api/v3/designer/maps")]
  [ApiController]
  public partial class MapsController : OlabController
  {
    public MapsController(ILogger<MapsController> logger, OLabDBContext context) : base(logger, context)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private static Model.Maps GetSimple(OLabDBContext context, uint id)
    {
      var phys = context.Maps.Include(x => x.SystemCounterActions).FirstOrDefault(x => x.Id == id);
      return phys;
    }

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
      logger.LogDebug($"GetMapNodeAsync(uint mapId={mapId}, nodeId={nodeId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<KeyValuePair<uint, uint>>.Result(new KeyValuePair<uint, uint>(mapId, nodeId));

      var map = await MapsReaderWriter.Instance(logger.GetLogger(), context).GetSingleAsync(mapId);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(Utils.Constants.ScopeLevelMap, mapId);

      MapsNodesFullRelationsDto dto;
      // get node with no wikitag translation
      dto = await GetNodeAsync(map, nodeId, false);

      if (!dto.Id.HasValue)
        return OLabNotFoundResult<uint>.Result(Utils.Constants.ScopeLevelNode, nodeId);

      return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
    }

    /// <summary>
    /// Get non-rendered nodes for a map
    /// </summary>
    /// <param name="mapId">Map id</param>
    /// <returns>IActionResult</returns>
    [HttpGet("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMapNodesAsync(uint mapId)
    {
      logger.LogDebug($"GetMapNodesAsync(uint mapId={mapId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<uint>.Result(mapId);

      var map = await MapsReaderWriter.Instance(logger.GetLogger(), context).GetSingleAsync(mapId);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(mapId);

      // get node with no wikitag translation
      var dtoList = await GetNodesAsync(map, false);
      return OLabObjectListResult<MapNodesFullDto>.Result(dtoList);
    }
    
    /// <summary>
    /// Create a new node link
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes/{nodeId}/links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodeLinkAsync(uint mapId, uint nodeId, [FromBody] PostNewLinkRequest body)
    {
      logger.LogDebug($"PostMapNodeLinkAsync( destinationId = {body.DestinationId})");

      try
      {
        // test if user has access to source map.
        var userContext = new UserContext(logger, context, HttpContext);
        if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
          return OLabUnauthorizedObjectResult<uint>.Result(mapId);

        var sourceNode = await GetMapNodeAsync(nodeId);
        if (sourceNode == null)
          return OLabNotFoundResult<uint>.Result(nodeId);

        var destinationNode = await GetMapNodeAsync(body.DestinationId);
        if (destinationNode == null)
          return OLabNotFoundResult<uint>.Result(body.DestinationId);

        var phys = MapNodeLinks.CreateDefault();
        phys.MapId = sourceNode.MapId;
        phys.NodeId1 = sourceNode.Id;
        phys.NodeId2 = destinationNode.Id;
        context.Entry(phys).State = EntityState.Added;

        await context.SaveChangesAsync();
       
        var dto = new PostNewLinkResponse {
          Id = phys.Id
        };

        return OLabObjectResult<PostNewLinkResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "PostMapNodeLinkAsync");
        throw ex;
      }
    }

    /// <summary>
    /// Create a new node
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodesAsync(PostNewNodeRequest body)
    {
      logger.LogDebug($"PostMapNodesAsync(x = {body.X}, y = {body.Y}, sourceId = {body.SourceId})");

      using var transaction = context.Database.BeginTransaction();

      try
      {
        var sourceNode = await GetMapNodeAsync(body.SourceId);
        if (sourceNode == null)
          return OLabNotFoundResult<uint>.Result(body.SourceId);

        // test if user has access to source map.
        var userContext = new UserContext(logger, context, HttpContext);
        if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, sourceNode.MapId))
          return OLabUnauthorizedObjectResult<uint>.Result(sourceNode.MapId);

        var phys = MapNodes.CreateDefault();
        phys.X = body.X;
        phys.Y = body.Y;
        phys.MapId = sourceNode.MapId;
        context.Entry(phys).State = EntityState.Added;

        await context.SaveChangesAsync();

        var link = MapNodeLinks.CreateDefault();
        link.MapId = sourceNode.MapId;
        link.NodeId1 = body.SourceId;
        link.NodeId2 = phys.Id;
        context.Entry(link).State = EntityState.Added;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        link.NodeId1Navigation = null;
        link.NodeId2Navigation = null;
        
        var dto = new PostNewNodeResponse {
          Links = link,
          Id = phys.Id
        };

        return OLabObjectResult<PostNewNodeResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        throw ex;
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
    {
      logger.LogDebug($"MapsController.GetScopedObjectsRawAsync(uint id={id})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, id))
        return OLabUnauthorizedObjectResult<uint>.Result(id);

      return await GetScopedObjectsAsync(id, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint id)
    {
      try
      {
        logger.LogDebug($"MapsController.GetScopedObjectsTranslatedAsync(uint id={id})");

        // test if user has access to map.
        var userContext = new UserContext(logger, context, HttpContext);
        if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, id))
          return OLabUnauthorizedObjectResult<uint>.Result(id);

        return await GetScopedObjectsAsync(id, true);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "GetScopedObjectsAsync");
        throw;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    private async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      var map = GetSimple(context, id);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(id);

      var phys = await GetScopedObjectsAllAsync(map.Id, Utils.Constants.ScopeLevelMap);
      var physServer = await GetScopedObjectsAllAsync(1, Utils.Constants.ScopeLevelServer);

      phys.Combine(physServer);

      phys.Constants.Add(new SystemConstants
      {
        Id = 0,
        Name = Utils.Constants.ReservedConstantMapId,
        ImageableId = map.Id,
        ImageableType = Utils.Constants.ScopeLevelMap,
        IsSystem = 1,
        Value = Encoding.ASCII.GetBytes(map.Id.ToString())
      });

      phys.Constants.Add(new SystemConstants
      {
        Id = 0,
        Name = Utils.Constants.ReservedConstantMapName,
        ImageableId = map.Id,
        ImageableType = Utils.Constants.ScopeLevelMap,
        IsSystem = 1,
        Value = Encoding.ASCII.GetBytes(map.Name)
      });

      phys.Constants.Add(new SystemConstants
      {
        Id = 0,
        Name = Utils.Constants.ReservedConstantSystemTime,
        ImageableId = 1,
        ImageableType = Utils.Constants.ScopeLevelNode,
        IsSystem = 1,
        Value = Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString() + " UTC")
      });

      var builder = new ObjectMapper.Designer.ScopedObjects(logger, enableWikiTranslation);
      var dto = builder.PhysicalToDto(phys);

      DecorateDto(dto);
      return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    private void DecorateDto(Dto.Designer.ScopedObjectsDto dto)
    {
      Type t = typeof(QuestionsController);
      RouteAttribute attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var questionRoute = attribute.Template;

      t = typeof(ConstantsController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var constantRoute = attribute.Template;

      t = typeof(CountersController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var counterRoute = attribute.Template;

      t = typeof(FilesController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var fileRoute = attribute.Template;

      foreach (var item in dto.Questions)
        item.Url = $"{BaseUrl}/{questionRoute}/{item.Id}";

      foreach (var item in dto.Counters)
        item.Url = $"{BaseUrl}/{counterRoute}/{item.Id}";

      foreach (var item in dto.Constants)
        item.Url = $"{BaseUrl}/{constantRoute}/{item.Id}";

      foreach (var item in dto.Files)
        item.Url = $"{BaseUrl}/{fileRoute}/{item.Id}";
    }
  }
}
