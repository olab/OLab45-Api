using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Endpoints.Player;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/servers")]
  public partial class ServerController : OlabController
  {
    private readonly ServerEndpoint _endpoint;

    public ServerController(ILogger<ServerController> logger, OLabDBContext context, HttpRequest request) : base(logger, context, request)
    {
      _endpoint = new ServerEndpoint(this.logger, context, auth);
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
      return await _endpoint.GetAsync(take, skip);
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
      return await _endpoint.GetScopedObjectsRawAsync(serverId);
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
      return await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
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
      return await _endpoint.GetScopedObjectsAsync(serverId, enableWikiTranslation);
    }
  }
}
