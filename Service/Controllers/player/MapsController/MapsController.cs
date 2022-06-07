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
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using OLabWebAPI.Model.ReaderWriter;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/maps")]
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
    /// Get a list of maps
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
    {
      logger.LogDebug($"GetAsync([FromQuery] int? take={take}, [FromQuery] int? skip={skip})");

      var items = new List<Model.Maps>();
      var total = 0;
      var remaining = 0;

      if (!skip.HasValue)
        skip = 0;

      if (take.HasValue && skip.HasValue)
      {
        items = await context.Maps.Skip(skip.Value).Take(take.Value).OrderBy(x => x.Name).ToListAsync();
        remaining = total - take.Value - skip.Value;
      }
      else
      {
        items = await context.Maps.OrderBy(x => x.Name).ToListAsync();
      }

      total = items.Count;

      logger.LogDebug(string.Format("found {0} maps", items.Count));

      var dtoList = new MapsMapper(logger).PhysicalToDto(items);

      // filter out any maps user does not have access to.
      var userContext = new UserContext(logger, context, HttpContext);
      dtoList = dtoList.Where(x => userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, x.Id)).ToList();
      return OLabObjectPagedListResult<MapsDto>.Result(dtoList, remaining);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync(uint id)
    {
      logger.LogDebug($"GetAsync(uint id={id})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, id))
        return OLabUnauthorizedObjectResult<uint>.Result(id);

      var map = GetSimple(context, id);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(id);

      var phys = await GetMapAsync(id);
      var dto = new MapsFullMapper(logger).PhysicalToDto(phys);

      return OLabObjectResult<MapsFullDto>.Result(dto);
    }

    /// <summary>
    /// Append template to an existing map
    /// </summary>
    /// <param name="mapId">Map to add template to</param>
    /// <param name="CreateMapRequest.templateId">Template to add to map</param>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostExtendMapAsync([FromRoute] uint mapId, [FromBody] ExtendMapRequest body)
    {
      logger.LogDebug($"MapsController.PostExtendMapAsync(mapId = {mapId}, templateId = {body.TemplateId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedResult.Result();

      try
      {
        var map = await context.Maps
          .AsNoTracking()
          .Include(x => x.MapNodes)
          .FirstOrDefaultAsync(x => x.Id == mapId);
        if (map == null)
          return OLabNotFoundResult<uint>.Result(mapId);

        var template = await context.Maps
          .AsNoTracking()
          .Include(x => x.MapNodes)
          .FirstOrDefaultAsync(x => x.Id == body.TemplateId);
        if (template == null)
          return OLabNotFoundResult<uint>.Result(body.TemplateId);

        map = await MapsReaderWriter.Instance(logger.GetLogger(), context)
          .CreateMapWithTemplateAsync(map, template);

        var mapLinks = context.MapNodeLinks.AsNoTracking().Where(x => x.MapId == map.Id).ToList();
        var linksDto = new MapNodeLinksMapper(logger).PhysicalToDto(mapLinks);

        var mapNodes = context.MapNodes.AsNoTracking().Where(x => x.MapId == map.Id).ToList();
        var nodesDto = new MapNodesFullMapper(logger).PhysicalToDto(mapNodes);

        var dto = new ExtendMapResponse
        {
          Nodes = nodesDto,
          Links = linksDto
        };

        return OLabObjectResult<ExtendMapResponse>.Result(dto);
      }
      catch (System.Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Create new map (using optional template)
    /// </summary>
    /// <param name="body">Create map request body</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostCreateMapAsync([FromBody] CreateMapRequest body)
    {
      logger.LogDebug($"MapsController.PostCreateMapAsync(templateId = {body.TemplateId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, 0))
        return OLabUnauthorizedResult.Result();

      try
      {
        Maps map = null;

        // if no templateId passed in, create default map
        if (!body.TemplateId.HasValue)
        {
          map = Maps.CreateDefault();
          context.Maps.Add(map);
          await context.SaveChangesAsync();
        }
        else
        {
          var templateMap = await context.Maps
            .AsNoTracking()
            .Include(x => x.MapNodes)
            .FirstOrDefaultAsync(x => x.Id == body.TemplateId);
          if (templateMap == null)
            return OLabNotFoundResult<uint>.Result(body.TemplateId.Value);

          map = await MapsReaderWriter.Instance(logger.GetLogger(), context)
            .CreateMapWithTemplateAsync(map, templateMap);
        }

        var dto = new MapsFullRelationsMapper(logger).PhysicalToDto(map);
        return OLabObjectResult<MapsFullRelationsDto>.Result(dto);
      }
      catch (System.Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mapdto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, MapsFullDto mapdto)
    {
      logger.LogDebug($"MapsController.PutAsync(id = {id})");

      var map = new MapsFullMapper(logger).DtoToPhysical(mapdto);

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
        return OLabUnauthorizedObjectResult<uint>.Result(id);

      if (id != map.Id)
        return OLabNotFoundResult<uint>.Result(id);

      context.Entry(map).State = EntityState.Modified;

      try
      {
        await context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        var existingMap = GetSimple(context, id);
        if (existingMap == null)
          return OLabNotFoundResult<uint>.Result(id);
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteAsync(uint id)
    {
      logger.LogDebug($"DeleteAsync(uint mapId={id})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("D", Utils.Constants.ScopeLevelMap, id))
        return OLabUnauthorizedObjectResult<uint>.Result(id);

      var map = GetSimple(context, id);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(id);

      context.Maps.Remove(map);
      await context.SaveChangesAsync();

      return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetLinksAsync(uint mapId)
    {
      logger.LogDebug($"GetLinksAsync(uint mapId={mapId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<uint>.Result(mapId);


      var map = GetSimple(context, mapId);
      if (map == null)
        return OLabNotFoundResult<uint>.Result(mapId);

      var items = await context.MapNodeLinks.Where(x => x.MapId == mapId).ToListAsync();
      logger.LogDebug(string.Format("found {0} MapNodeLinks", items.Count));

      var dtoList = new MapNodeLinksFullMapper(logger).PhysicalToDto(items);

      return OLabObjectListResult<MapNodeLinksFullDto>.Result(dtoList);
    }

    [HttpOptions]
    public void Options()
    {

    }
  }
}
