using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints.Player;
using OLab.Data.Models;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;
using OLab.Data.Dtos;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/servers")]
public partial class ServerController : OLabController
{
  private readonly ServerEndpoint _endpoint;

  public ServerController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));
    Guard.Argument(fileStorageProvider).NotNull(nameof(fileStorageProvider));

    Logger = OLabLogger.CreateNew<ServerEndpoint>(loggerFactory);
    _endpoint = new ServerEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider);
  }

  /// <summary>
  /// ReadAsync a list of servers
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var pagedResponse = await _endpoint.GetAsync(take, skip);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<Servers>.Result(pagedResponse.Data));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsRawAsync(serverId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }
}
