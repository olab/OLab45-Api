using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Controllers.Player;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Model;
using OLabWebAPI.Model.ReaderWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Controllers.Designer
{
  [Route("olab/api/v3/designer/maps")]
  [ApiController]
  public partial class MapsController : OlabController
  {
    private readonly MapsEndpoint _endpoint;

    public MapsController(ILogger<ConstantsController> logger, OLabDBContext context, HttpRequest request) : base(logger, context, request)
    {
      _endpoint = new MapsEndpoint(this.logger, context, auth);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private Model.Maps GetSimple(OLabDBContext context, uint id)
    {
      return _endpoint.GetSimple(context, id);
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
      return await _endpoint.GetMapNodeAsync(mapId, nodeId);
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
      return await _endpoint.GetMapNodesAsync(mapId);
    }

    /// <summary>
    /// Create a new node link
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes/{nodeId}/links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodeLinkAsync(uint mapId, uint nodeId, [FromBody] PostNewLinkRequest body)
    {
      return await _endpoint.PostMapNodeLinkAsync(mapId, nodeId, body);
    }

    /// <summary>
    /// Create a new node
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpPost("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostMapNodesAsync(PostNewNodeRequest body)
    {
      return await _endpoint.PostMapNodesAsync(body);
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
        var dto = await _endpoint.GetScopedObjectsRawAsync(id);
        if (dto == null)
          return OLabNotFoundResult<uint>.Result(id);
        return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
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
        var dto = await _endpoint.GetScopedObjectsAsync(id);
        if (dto == null)
          return OLabNotFoundResult<uint>.Result(id);
        return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
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
      var dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
      if (dto == null)
        return OLabNotFoundResult<uint>.Result(id);

      DecorateDto(dto);
      return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
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
