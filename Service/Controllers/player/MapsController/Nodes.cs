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
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using OLabWebAPI.Model.ReaderWriter;
using System;

namespace OLabWebAPI.Controllers.Player
{
  public partial class MapsController : OlabController
  {
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
      if (nodeId > 0)
        dto = await GetNodeAsync(map, nodeId);
      else
        dto = await GetRootNodeAsync(map);

      if (!dto.Id.HasValue)
        return OLabNotFoundResult<uint>.Result(Utils.Constants.ScopeLevelNode, nodeId);

      // test if end node, meaning we can close the session.  otherwise
      // record the OnPlay event
      if (dto.End.HasValue && dto.End.Value)
      {
        userContext.Session.OnPlayNode(userContext.SessionId, map.Id, dto.Id.Value);
        userContext.Session.OnEndSession(userContext.SessionId, map.Id, dto.Id.Value);
      }
      else
      {
        if (nodeId == 0)
        {
          userContext.Session.OnStartSession(userContext.UserName, map.Id, userContext.IPAddress);
          dto.SessionId = userContext.Session.GetSessionId();
          userContext.SessionId = dto.SessionId;
        }

        userContext.Session.OnPlayNode(userContext.SessionId, map.Id, dto.Id.Value);
      }

      UpdateNodeCounter();

      return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
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
      logger.LogDebug($"DeleteNodeAsync(uint mapId={mapId}, nodeId={nodeId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<uint>.Result(mapId);

      using var transaction = context.Database.BeginTransaction();

      try
      {
        var links = context.MapNodeLinks.Where( x => ( x.NodeId1 == nodeId ) || ( x.NodeId2 == nodeId )).ToArray();
        logger.LogDebug($"deleting {links.Count()} links");
        context.MapNodeLinks.RemoveRange( links );

        var node = await context.MapNodes.FirstOrDefaultAsync( x => x.Id == nodeId );
        context.MapNodes.Remove(node);
        logger.LogDebug($"deleting node id: {node.Id}");
       
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        var responseDto = new MapNodesPostResponseDto
        {
          Id = nodeId
        };

        return OLabObjectResult<MapNodesPostResponseDto>.Result(responseDto);

      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        throw ex;
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
      logger.LogDebug($"PutNodeAsync(uint mapId={mapId}, nodeId={nodeId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<uint>.Result(mapId);

      using var transaction = context.Database.BeginTransaction();

      try
      {
        var builder = new ObjectMapper.MapNodesFullMapper(logger);
        var phys = builder.DtoToPhysical(dto);

        context.MapNodes.Update(phys);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        var responseDto = new MapNodesPostResponseDto
        {
          Id = nodeId
        };

        return OLabObjectResult<MapNodesPostResponseDto>.Result(responseDto);

      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        throw ex;
      }


    }

    /// <summary>
    /// Get node for map
    /// </summary>
    /// <param name="map">Map object</param>
    /// <returns>MapsNodesFullRelationsDto</returns>
    private async Task<MapsNodesFullRelationsDto> GetRootNodeAsync(Model.Maps map)
    {
      var phys = await context.MapNodes
        .FirstOrDefaultAsync(x => x.MapId == map.Id && x.TypeId.Value == (int)Model.MapNodes.NodeType.RootNode);

      if (phys == null)
      {
        // if no map node by this point, then the map doesn't have a root node
        // defined so take the first one (by id)        
        phys = await context.MapNodes.Where(x => x.MapId == map.Id).OrderBy(x => x.Id).FirstOrDefaultAsync();
      }

      return await GetNodeAsync(map, phys.Id);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateNodeCounter()
    {
      var counter = context.SystemCounters.Where(x => x.Name == "nodeCounter").FirstOrDefault();

      var value = counter.ValueAsNumber();

      value++;
      counter.ValueFromNumber(value);

      context.SystemCounters.Update(counter);
      context.SaveChanges();
    }

  }

}
