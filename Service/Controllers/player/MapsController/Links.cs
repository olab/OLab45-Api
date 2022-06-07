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
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private static Model.MapNodeLinks GetLinkSimple(OLabDBContext context, uint id)
    {
      var phys = context.MapNodeLinks.FirstOrDefault(x => x.Id == id);
      return phys;
    }

    /// <summary>
    /// Saves a link edit
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <param name="linkId">link id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{mapId}/nodes/{nodeId}/links/{linkId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutMapNodeLinksAsync(uint mapId, uint nodeId, uint linkId, [FromBody] MapNodeLinksFullDto linkdto)
    {
      logger.LogDebug($"PutMapNodeLinksAsync(uint mapId={mapId}, nodeId={nodeId}, linkId={linkId})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("W", Utils.Constants.ScopeLevelMap, mapId))
        return OLabUnauthorizedObjectResult<KeyValuePair<uint, uint>>.Result(new KeyValuePair<uint, uint>(mapId, nodeId));

      try
      {
        var builder = new MapNodeLinksFullMapper(logger);
        var phys = builder.DtoToPhysical(linkdto);

        context.Entry(phys).State = EntityState.Modified;
        await context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        var existingMap = GetLinkSimple(context, linkId);
        if (existingMap == null)
          return OLabNotFoundResult<uint>.Result(linkId);
        else
        {
          throw;
        }
      }

      return NoContent();

    }

  }
  
}
