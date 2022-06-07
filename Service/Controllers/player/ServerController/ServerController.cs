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
  [Route("olab/api/v3/servers")]
  public partial class ServerController : OlabController
  {
    public ServerController(ILogger<NodesController> logger, OLabDBContext context) : base(logger, context)
    {
    }

    /// <summary>
    /// Get a list of servers
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
    {
      var items = new List<Servers>();
      var total = 0;
      var remaining = 0;

      if (!skip.HasValue)
        skip = 0;

      if (take.HasValue && skip.HasValue)
      {
        items = await context.Servers.Skip(skip.Value).Take(take.Value).OrderBy(x => x.Name).ToListAsync();
        remaining = total - take.Value - skip.Value;
      }
      else
      {
        items = await context.Servers.OrderBy(x => x.Name).ToListAsync();
      }

      total = items.Count;

      logger.LogDebug(string.Format("found {0} servers", items.Count));

      // filter out any maps user does not have access to.
      var userContext = new UserContext(logger, context, HttpContext);
      return OLabObjectPagedListResult<Servers>.Result(items, remaining);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverId"></param>
    /// <returns></returns>
    [HttpGet("{serverId}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint serverId)
    {
      logger.LogDebug($"ServerController.GetScopedObjectsRawAsync(uint serverId={serverId})");
      var dto = await GetScopedObjectsAsync(serverId, false);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);      

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverId"></param>
    /// <returns></returns>
    [HttpGet("{serverId}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsTranslatedAsync(uint serverId)
    {
      logger.LogDebug($"ServerController.GetScopedObjectsTranslatedAsync(uint serverId={serverId})");
      var dto = await GetScopedObjectsAsync(serverId, true);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);      
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    private async Task<ScopedObjectsDto> GetScopedObjectsAsync(
      uint serverId,
      bool enableWikiTranslation)
    {
      logger.LogDebug($"ServerController.GetScopedObjectsAsync(uint serverId={serverId})");

      var phys = await GetScopedObjectsAllAsync(serverId, Utils.Constants.ScopeLevelServer);
      var builder = new ObjectMapper.ScopedObjects(logger, enableWikiTranslation);
      var dto = builder.PhysicalToDto(phys);

      return dto;
    }
  }
}
