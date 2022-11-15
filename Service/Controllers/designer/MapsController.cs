using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Endpoints.WebApi.Player;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Designer
{
  [Route("olab/api/v3/designer/maps")]
  [ApiController]
  public partial class MapsController : OlabController
  {
    private readonly MapsEndpoint _endpoint;

    public MapsController(ILogger<ConstantsController> logger, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new MapsEndpoint(this.logger, context);
    }

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
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);
        return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get non-rendered nodes for a map
    /// </summary>
    /// <param name="mapId">Map id</param>
    /// <returns>IActionResult</returns>
    [HttpGet("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMapNodesAsync(uint mapId)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dtoList = await _endpoint.GetMapNodesAsync(auth, mapId);
        return OLabObjectListResult<MapNodesFullDto>.Result(dtoList);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Create a new node link
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes/{nodeId}/links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodeLinkAsync(uint mapId, uint nodeId, [FromBody] PostNewLinkRequest body)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.PostMapNodeLinkAsync(auth, mapId, nodeId, body);
        return OLabObjectResult<PostNewLinkResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Create a new node
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodesAsync(PostNewNodeRequest body)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.PostMapNodesAsync(auth, body);
        return OLabObjectResult<PostNewNodeResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint id)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    private async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      try
      {
        var dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
        DecorateDto(dto);
        return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if ( ex is OLabUnauthorizedException )
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    private void DecorateDto(Dto.Designer.ScopedObjectsDto dto)
    {
      Type t = typeof(QuestionsController);
      RouteAttribute attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var questionRoute = attribute.Template;

      t = typeof(ConstantsController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var constantRoute = attribute.Template;

      t = typeof(CountersController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var counterRoute = attribute.Template;

      t = typeof(FilesController);
      attribute =
          (RouteAttribute)Attribute.GetCustomAttribute(t, typeof(RouteAttribute));
      var fileRoute = attribute.Template;

      foreach (var item in dto.Questions)
        item.Url = $"{BaseUrl}/{questionRoute}/{item.Id}";

      foreach (var item in dto.Counters)
        item.Url = $"{BaseUrl}/{counterRoute}/{item.Id}";

      foreach (var item in dto.Constants)
        item.Url = $"{BaseUrl}/{constantRoute}/{item.Id}";

      foreach (var item in dto.Files)
        item.Url = $"{BaseUrl}/{fileRoute}/{item.Id}";
    }
  }
}
