using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OLabWebAPI.Common;

namespace OLabWebAPI.Controllers.Player
{
  public partial class MapsController : OlabController
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/nodes/{nodeId}/dynamicobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetDynamicScopedObjectsRawAsync(uint mapId, uint nodeId, [FromQuery] uint sinceTime = 0)
    {
      logger.LogDebug($"DynamicScopedObjectsController.GetDynamicScopedObjectsRawAsync({mapId}, {nodeId}, {sinceTime})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<KeyValuePair<uint, uint>>.Result(new KeyValuePair<uint, uint>(mapId, nodeId));

      var node = await GetMapRootNode(mapId, nodeId);
      return await GetDynamicScopedObjectsAsync(1, node, sinceTime, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/nodes/{nodeId}/dynamicobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetDynamicScopedObjectsTranslatedAsync(uint mapId, uint nodeId, [FromQuery] uint sinceTime = 0)
    {
      logger.LogDebug($"DynamicScopedObjectsController.GetDynamicScopedObjectsTranslatedAsync({mapId}, {nodeId}, {sinceTime})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<KeyValuePair<uint, uint>>.Result(new KeyValuePair<uint, uint>(mapId, nodeId));

      var node = await GetMapRootNode(mapId, nodeId);
      return await GetDynamicScopedObjectsAsync(1, node, sinceTime, true);
    }

    /// <summary>
    /// Retrieve dynamic scoped objects for current node
    /// </summary>
    /// <param name="serverId">Server id</param>
    /// <param name="node">Current node</param>
    /// <param name="sinceTime">Look for changes since</param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    public async Task<IActionResult> GetDynamicScopedObjectsAsync(
      uint serverId,
      Model.MapNodes node,
      uint sinceTime,
      bool enableWikiTranslation)
    {
      var physServer = new Model.ScopedObjects();
      var physNode = new Model.ScopedObjects();
      var physMap = new Model.ScopedObjects();

      physServer.Counters = await GetScopedCountersAsync(Utils.Constants.ScopeLevelServer, serverId);
      physNode.Counters = await GetScopedCountersAsync(Utils.Constants.ScopeLevelNode, node.Id);
      physMap.Counters = await GetScopedCountersAsync(Utils.Constants.ScopeLevelMap, node.MapId);


      await ProcessNodeCounters(node, physMap.Counters);

      var builder = new ObjectMapper.DynamicScopedObjects(logger, enableWikiTranslation);

      var dto = builder.PhysicalToDto(physServer, physMap, physNode);
      return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
    }

    /// <summary>
    /// Apply MapNodeCounter expressions to counters
    /// </summary>
    /// <param name="node">Current node</param>
    /// <param name="counters">Raw system (map-level) counters</param>
    /// <returns>void</returns>
    private async Task ProcessNodeCounters(Model.MapNodes node, IList<SystemCounters> counters)
    {
      var counterActions = await context.SystemCounterActions.Where(x =>
        (x.ImageableId == node.Id) &&
        (x.ImageableType == Utils.Constants.ScopeLevelNode) &&
        (x.OperationType == "open")).ToListAsync();

      logger.LogDebug($"Found {counterActions.Count} counterActions records for node {node.Id} ");

      foreach (var counterAction in counterActions)
      {
        var rawCounter = counters.FirstOrDefault(x => x.Id == counterAction.CounterId);
        if (rawCounter == null)
          logger.LogError($"Enable to lookup counter {counterAction.CounterId} in action {counterAction.Id}");
        else if (counterAction.ApplyFunctionToCounter(rawCounter))
          logger.LogDebug($"Updated counter id {rawCounter.Id} with function '{counterAction.Expression}'. now = {rawCounter.ValueAsString()}");
      }

      return;
    }
  }
}