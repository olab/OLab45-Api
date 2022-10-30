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
using System;
using OLabWebAPI.Common.Exceptions;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/servers")]
  public partial class ServerController : OlabController
  {
    private readonly ServerEndpoint _endpoint;

    public ServerController(ILogger<ServerController> logger, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new ServerEndpoint(this.logger, context);
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
      try
      {
        var pagedResponse = await _endpoint.GetAsync(take, skip);
        return OLabObjectListResult<Servers>.Result(pagedResponse.Data);
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [HttpGet("{serverId}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint serverId)
    {
      try
      {
        var dto = await _endpoint.GetScopedObjectsRawAsync(serverId);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [HttpGet("{serverId}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsTranslatedAsync(uint serverId)
    {
      try
      {
        var dto = await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
