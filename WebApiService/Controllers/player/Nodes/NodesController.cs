using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using Dawn;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/nodes")]
[ApiController]
public partial class NodesController : OLabController
{
  private readonly NodesEndpoint _endpoint;

  public NodesController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      userService,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Logger = OLabLogger.CreateNew<NodesController>(loggerFactory);

    _endpoint = new NodesEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider);
  }

  /// <summary>
  /// Get full map node, with relations
  /// </summary>
  /// <param name="nodeId">Node id (0, if root node)</param>
  /// <returns>MapsNodesFullRelationsDto response</returns>
  [HttpGet("{nodeId}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetNodeTranslatedAsync(uint nodeId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      MapsNodesFullRelationsDto dto = await _endpoint.GetNodeTranslatedAsync(auth, nodeId);
      return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <param name="dto"></param>
  /// <returns></returns>
  [HttpPut("{nodeId}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PutNodeAsync(uint id, [FromBody] MapNodesFullDto dto)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      await _endpoint.PutNodeAsync(auth, id, dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

    return NoContent();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  [HttpPost("{nodeId}/links")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostLinkAsync(
    uint nodeId,
    [FromBody] MapNodeLinksPostDataDto data
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      MapNodeLinksPostResponseDto dto = await _endpoint.PostLinkAsync(auth, nodeId, data);
      return OLabObjectResult<MapNodeLinksPostResponseDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  [HttpPost("{nodeId}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostNodeAsync(
    uint mapId,
    [FromBody] MapNodesPostDataDto data
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      MapNodesPostResponseDto dto = await _endpoint.PostNodeAsync(auth, mapId, data);
      return OLabObjectResult<MapNodesPostResponseDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }
}
