using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.ObjectMapper;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi
{
  public class OLabController : ControllerBase
  {
    protected readonly OLabDBContext DbContext;

    //  this is set in derived classes
    protected IOLabLogger Logger = null;

    protected string Token;
    //protected readonly IUserService _userService;
    protected IUserContext userContext;
    protected readonly IOLabConfiguration _configuration;
    protected readonly IOLabModuleProvider<IWikiTagModule> _wikiTagProvider;
    protected readonly IOLabModuleProvider<IFileStorageModule> _fileStorageProvider;

    protected string BaseUrl => $"{Request.Scheme}://{Request.Host.Value}";
    protected string RequestPath => $"{Request.Path.ToString().Trim('/')}";

    public OLabController(
      IOLabConfiguration configuration,
      //IUserService _userService,
      OLabDBContext dbContext)
    {
      //Guard.Argument(_userService).NotNull(nameof(_userService));
      Guard.Argument(configuration).NotNull(nameof(configuration));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));

      _configuration = configuration;

      DbContext = dbContext;
    }

    public OLabController(
      IOLabConfiguration configuration,
      //IUserService _userService,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : this(configuration, dbContext)
    {
      Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));
      Guard.Argument(fileStorageProvider).NotNull(nameof(fileStorageProvider));

      _wikiTagProvider = wikiTagProvider;
      _fileStorageProvider = fileStorageProvider;
    }

    [NonAction]
    protected ContentResult Result(IActionResult actionResult)
    {
      var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(actionResult);
      var content = Content(jsonString, "application/json", Encoding.UTF8);
      content.StatusCode = 500;
      return content;
    }

    /// <summary>
    /// Get the _authentication context from the host context
    /// </summary>
    /// <param name="hostContext">Function context</param>
    /// <returns>IOLabAuthentication</returns>
    /// <exception cref="Exception"></exception>
    [NonAction]
    protected IOLabAuthorization GetAuthorization(HttpContext hostContext)
    {
      // Get the item set by the middleware
      if (hostContext.Items.TryGetValue("usercontext", out var value) && value is IUserContext userContext)
      {
        Logger.LogInformation($"User context: {userContext}");

        var auth = new OLabAuthorization(Logger, DbContext);
        auth.ApplyUserContext(userContext);

        return auth;
      }

      throw new Exception("unable to get auth RequestContext");

    }

    [NonAction]
    protected async ValueTask<Maps> GetMapAsync(uint id)
    {
      var phys = await DbContext.Maps.FirstOrDefaultAsync(x => x.Id == id);
      DbContext.Entry(phys).Collection(b => b.MapNodes).Load();
      return phys;
    }

    [NonAction]
    protected async Task<MapNodes> GetMapRootNode(uint mapId, uint nodeId)
    {
      if (nodeId != 0)
        return await DbContext.MapNodes
          .Where(x => x.MapId == mapId && x.Id == nodeId)
          .FirstOrDefaultAsync(x => x.Id == nodeId);

      var item = await DbContext.MapNodes
          .Where(x => x.MapId == mapId && x.TypeId == 1)
          .FirstOrDefaultAsync(x => x.Id == nodeId);

      if (item == null)
        item = await DbContext.MapNodes
                  .Where(x => x.MapId == mapId)
                  .OrderBy(x => x.Id)
                  .FirstAsync();

      return item;
    }

    /// <summary>
    /// Get nodes for map
    /// </summary>
    /// <param name="map">Parent map to query for</param>
    /// <param name="enableWikiTanslation">PErform WikiTag translation</param>
    /// <returns>List of mapnode dto's</returns>
    [NonAction]
    protected async Task<IList<MapNodesFullDto>> GetNodesAsync(Maps map, bool enableWikiTanslation = true)
    {
      var physList = await DbContext.MapNodes.Where(x => x.MapId == map.Id).ToListAsync();
      Logger.LogDebug(string.Format("found {0} mapNodes", physList.Count));

      var dtoList = new MapNodesFullMapper(Logger, enableWikiTanslation).PhysicalToDto(physList);
      return dtoList;
    }

    /// <summary>
    /// Get node for map
    /// </summary>
    /// <param name="map">Map object</param>
    /// <param name="nodeId">Node id</param>
    /// <param name="enableWikiTanslation">PErform WikiTag translation</param>
    /// <returns>MapsNodesFullRelationsDto</returns>
    //[NonAction]
    //protected async Task<MapsNodesFullRelationsDto> GetNodeAsync(Maps map, uint nodeId, bool enableWikiTanslation = true)
    //{
    //  MapNodes phys = await DbContext.MapNodes
    //    .FirstOrDefaultAsync(x => x.MapId == map.Id && x.Id == nodeId);

    //  if (phys == null)
    //    return new MapsNodesFullRelationsDto();

    //  // explicitly load the related objects.
    //  DbContext.Entry(phys).Collection(b => b.MapNodeLinksNodeId1Navigation).Load();

    //  var builder = new MapsNodesFullRelationsMapper(Logger, enableWikiTanslation);
    //  MapsNodesFullRelationsDto dto = builder.PhysicalToDto(phys);

    //  var linkedIds = phys.MapNodeLinksNodeId1Navigation.Select(x => x.NodeId2).Distinct().ToList();
    //  var linkedNodes = DbContext.MapNodes.Where(x => linkedIds.Contains(x.Id)).ToList();

    //  foreach (MapNodeLinksDto item in dto.MapNodeLinks)
    //  {
    //    MapNodes link = linkedNodes.Where(x => x.Id == item.DestinationId).FirstOrDefault();
    //    item.DestinationTitle = linkedNodes.Where(x => x.Id == item.DestinationId).Select(x => x.Title).FirstOrDefault();
    //    if (string.IsNullOrEmpty(item.LinkText))
    //      item.LinkText = item.DestinationTitle;
    //  }

    //  return dto;
    //}

    /// <summary>
    /// Get a mapnode
    /// </summary>
    /// <param name="nodeId">Node id</param>
    /// <returns></returns>
    [NonAction]
    public async ValueTask<MapNodes> GetMapNodeAsync(uint nodeId)
    {
      var item = await DbContext.MapNodes
          .FirstOrDefaultAsync(x => x.Id == nodeId);

      // explicitly load the related objects.
      DbContext.Entry(item).Collection(b => b.MapNodeLinksNodeId1Navigation).Load();

      return item;
    }

    /// <summary>
    /// Get question response
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<SystemQuestionResponses> GetQuestionResponseAsync(uint id)
    {
      var item = await DbContext.SystemQuestionResponses.FirstOrDefaultAsync(x => x.Id == id);
      return item;
    }

    /// <summary>
    /// Get constant
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<SystemConstants> GetConstantAsync(uint id)
    {
      var item = await DbContext.SystemConstants
          .FirstOrDefaultAsync(x => x.Id == id);
      return item;
    }

    /// <summary>
    /// Get file
    /// </summary>
    /// <param name="id">file id</param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<SystemFiles> GetFileAsync(uint id)
    {
      var item = await DbContext.SystemFiles
          .FirstOrDefaultAsync(x => x.Id == id);
      return item;
    }

    /// <summary>
    /// Get question
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<SystemQuestions> GetQuestionSimpleAsync(uint id)
    {
      var item = await DbContext.SystemQuestions
          .FirstOrDefaultAsync(x => x.Id == id);
      return item;
    }

    /// <summary>
    /// Get question with responses
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<SystemQuestions> GetQuestionAsync(uint id)
    {
      var item = await DbContext.SystemQuestions
          .Include(x => x.SystemQuestionResponses)
          .FirstOrDefaultAsync(x => x.Id == id);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="sinceTime"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<OLab.Api.Model.ScopedObjects> GetScopedObjectsDynamicAsync(
      uint parentId,
      uint sinceTime,
      string scopeLevel)
    {
      var phys = new OLab.Api.Model.ScopedObjects
      {
        Counters = await GetScopedCountersAsync(scopeLevel, parentId, sinceTime)
      };

      return phys;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async ValueTask<OLab.Api.Model.ScopedObjects> GetScopedObjectsAllAsync(
      uint parentId,
      string scopeLevel)
    {
      var phys = new OLab.Api.Model.ScopedObjects
      {
        Constants = await GetScopedConstantsAsync(parentId, scopeLevel),
        Questions = await GetScopedQuestionsAsync(parentId, scopeLevel),
        Files = await GetScopedFilesAsync(parentId, scopeLevel),
        Scripts = await GetScopedScriptsAsync(parentId, scopeLevel),
        Themes = await GetScopedThemesAsync(parentId, scopeLevel),
        Counters = await GetScopedCountersAsync(scopeLevel, parentId, 0)
      };

      if (scopeLevel == OLab.Api.Utils.Constants.ScopeLevelMap)
      {
        var items = new List<SystemCounterActions>();
        items.AddRange(await DbContext.SystemCounterActions.Where(x =>
            x.MapId == parentId).ToListAsync());

        phys.CounterActions.AddRange(items);
      }

      return phys;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async Task<List<SystemConstants>> GetScopedConstantsAsync(uint parentId, string scopeLevel)
    {
      var items = new List<SystemConstants>();

      items.AddRange(await DbContext.SystemConstants.Where(x =>
        x.ImageableType == scopeLevel && x.ImageableId == parentId).ToListAsync());

      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async Task<List<SystemFiles>> GetScopedFilesAsync(uint parentId, string scopeLevel)
    {
      var items = new List<SystemFiles>();

      items.AddRange(await DbContext.SystemFiles.Where(x =>
        x.ImageableType == scopeLevel && x.ImageableId == parentId).ToListAsync());

      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async Task<List<SystemQuestions>> GetScopedQuestionsAsync(uint parentId, string scopeLevel)
    {
      var items = new List<SystemQuestions>();

      items.AddRange(await DbContext.SystemQuestions
        .Where(x => x.ImageableType == scopeLevel && x.ImageableId == parentId)
        .Include("SystemQuestionResponses")
        .ToListAsync());

      // order the responses by Order field
      foreach (var item in items)
        item.SystemQuestionResponses = item.SystemQuestionResponses.OrderBy(x => x.Order).ToList();

      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async Task<List<SystemThemes>> GetScopedThemesAsync(uint parentId, string scopeLevel)
    {
      var items = new List<SystemThemes>();

      items.AddRange(await DbContext.SystemThemes.Where(x =>
        x.ImageableType == scopeLevel && x.ImageableId == parentId).ToListAsync());

      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="scopeLevel"></param>
    /// <returns></returns>
    [NonAction]
    protected async Task<List<SystemScripts>> GetScopedScriptsAsync(uint parentId, string scopeLevel)
    {
      var items = new List<SystemScripts>();

      items.AddRange(await DbContext.SystemScripts.Where(x =>
        x.ImageableType == scopeLevel && x.ImageableId == parentId).ToListAsync());

      return items;
    }

    /// <summary>
    /// Get counter 
    /// </summary>
    /// <param name="id">counter id</param>
    /// <returns>Counter</returns>
    [NonAction]
    protected async Task<SystemCounters> GetCounterAsync(uint id)
    {
      var phys = await DbContext.SystemCounters.SingleOrDefaultAsync(x => x.Id == id);
      if (phys.Value == null)
        phys.Value = new List<byte>().ToArray();
      if (phys.StartValue == null)
        phys.StartValue = new List<byte>().ToArray();
      return phys;
    }

    /// <summary>
    /// Get counters associated with a 'parent' object 
    /// </summary>
    /// <param name="scopeLevel">Scope level of parent (Maps, MapNodes, etc)</param>
    /// <param name="parentId">Id of parent object</param>
    /// <param name="sinceTime">(optional) looks for values changed since a (unix) time</param>
    /// <returns>List of counters</returns>
    [NonAction]
    protected async Task<List<SystemCounters>> GetScopedCountersAsync(string scopeLevel, uint parentId, uint sinceTime = 0)
    {
      var items = new List<SystemCounters>();

      if (sinceTime != 0)
      {
        // generate DateTime from sinceTime
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(sinceTime).ToLocalTime();
        items.AddRange(await DbContext.SystemCounters.Where(x =>
          x.ImageableType == scopeLevel && x.ImageableId == parentId && x.UpdatedAt >= dateTime).ToListAsync());
      }
      else
      {
        items.AddRange(await DbContext.SystemCounters.Where(x =>
          x.ImageableType == scopeLevel && x.ImageableId == parentId).ToListAsync());
      }

      return items;
    }
  }
}