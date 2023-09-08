using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Endpoints.WebApi.Player;
using OLab.Api.Model;
using OLab.Api.Services;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace OLabWebAPI.Endpoints.WebApi.Designer
{
  [Route("olab/api/v3/designer/maps")]
  [ApiController]
  public partial class MapsController : OlabController
  {
    private readonly MapsEndpoint _endpoint;

    public MapsController(ILogger<ConstantsController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new MapsEndpoint(this.logger, appSettings, context);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        MapsNodesFullRelationsDto dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);
        return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        System.Collections.Generic.IList<MapNodesFullDto> dtoList = await _endpoint.GetMapNodesAsync(auth, mapId);
        return OLabObjectListResult<MapNodesFullDto>.Result(dtoList);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        PostNewLinkResponse dto = await _endpoint.PostMapNodeLinkAsync(auth, mapId, nodeId, body);
        return OLabObjectResult<PostNewLinkResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Deletes a node link
    /// </summary>
    /// <param name="mapId">Map id</param>
    /// <param name="linkId">Link id</param>
    /// <returns>IActionResult</returns>
    [HttpDelete("{mapId}/links/{linkId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteMapNodeLinkAsync(uint mapId, uint linkId)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        var deleted = await _endpoint.DeleteMapNodeLinkAsync(auth, mapId, linkId);
        return OLabObjectResult<bool>.Result(deleted);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        PostNewNodeResponse dto = await _endpoint.PostMapNodesAsync(auth, body);
        return OLabObjectResult<PostNewNodeResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Update a given map's nodegrid
    /// </summary>
    /// <param name="mapId">Map id</param>
    /// <param name="body">nodegrid DTO</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{mapId}/nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutMapNodegridAsync(uint mapId, PutNodeGridRequest[] body)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        bool dto = await _endpoint.PutMapNodegridAsync(auth, mapId, body);
        return OLabObjectResult<bool>.Result(dto);
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
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        Dto.Designer.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.Designer.ScopedObjectsDto>.Result(dto);
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
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint id)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        Dto.Designer.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.Designer.ScopedObjectsDto>.Result(dto);
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
    /// <param name="id"></param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    private async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      try
      {
        Dto.Designer.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
        DecorateDto(dto);
        return OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get a list of users
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/securityusers/candidates")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetMapAccessCandidates(
      uint mapId,
      #nullable enable
      [FromQuery] string? search
      #nullable disable
    )
    {
      try
      {
        Maps map = dbContext.Maps.Find(mapId);

        if (map == null)
          throw new OLabObjectNotFoundException(Utils.Constants.ScopeLevelMap, mapId);

        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        // only allow users with write-access to list olab users users from this endpoint
        if (!auth.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
          throw new OLabUnauthorizedException(Utils.Constants.ScopeLevelMap, map.Id);

        var dtos = _endpoint.GetMapAccessCandidates(map, search ?? "");

        var list = new List<Hashtable>();

        foreach( var user in dtos )
        {
          list.Add(new Hashtable
          {
            { "id", user.Id },
            { "email", user.Email },
            { "username", user.Username },
            { "nickname", user.Nickname },
          });
        }

        return OLabObjectResult<IList<Hashtable>>.Result(list);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Insert a user to the users table
    /// </summary>
    /// <param name="mapId">Relevent map object</param>
    /// <returns></returns>
    [HttpPut("{mapId}/securityusers/candidates")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> InsertMapAccessCandidateAsync(uint mapId, [FromBody] MapAccessCandidateRequest body)
    {
      try
      {
        Maps map = dbContext.Maps.Find(mapId);

        if (map == null)
          throw new OLabObjectNotFoundException(Utils.Constants.ScopeLevelMap, mapId);

        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        // only allow users with write-access to list olab users users from this endpoint
        if (!auth.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
          throw new OLabUnauthorizedException(Utils.Constants.ScopeLevelMap, map.Id);

        body.MapId = map.Id;

        var result = await _endpoint.PutMapAccessCandidateAsync(map, body);

        return OLabObjectResult<int>.Result(result);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get a list of security users for a given map
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    [HttpGet("{mapId}/securityusers")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetSecurityUsers(uint mapId)
    {
      try
      {
        Maps map = dbContext.Maps.Find(mapId);

        if (map == null)
          throw new OLabObjectNotFoundException(Utils.Constants.ScopeLevelMap, mapId);

        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        // only allow users with write-access to list map security users
        if (!auth.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
          throw new OLabUnauthorizedException(Utils.Constants.ScopeLevelMap, map.Id);

        var dtos = _endpoint.GetSecurityUsersRaw(map);

        var list = new List<Hashtable>();

        foreach( var rule in dtos )
        {
          var user = dbContext.Users.Where(x => x.Id == rule.UserId).FirstOrDefault();

          list.Add(new Hashtable
          {
            { "userId", rule.UserId },
            { "acl", rule.Acl },
            { "user", user != null ? new Hashtable
              {
                { "id", user.Id },
                { "email", user.Email },
                { "username", user.Username },
                { "nickname", user.Nickname },
              } : null
            },
          });
        }

        return OLabObjectResult<IList<Hashtable>>.Result(list);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Assign a security user to a given map
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    [HttpPost("{mapId}/securityusers")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SetMapSecurityUserAsync(uint mapId, [FromBody] AssignSecurityUserRequest body)
    {
      try
      {
        Maps map = dbContext.Maps.Find(mapId);

        if (map == null)
          throw new OLabObjectNotFoundException(Utils.Constants.ScopeLevelMap, mapId);

        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        // only allow users with write-access to list map security users
        if (!auth.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
          throw new OLabUnauthorizedException(Utils.Constants.ScopeLevelMap, map.Id);

        var result = await _endpoint.SetMapSecurityUserAsync(map, body);

        return OLabObjectResult<bool>.Result(result);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Unassign a security user from a given map
    /// </summary>
    /// <param name="mapId">Map ID</param>
    /// <param name="userId">User ID</param>
    /// <returns></returns>
    [HttpDelete("{mapId}/securityusers/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UnsetMapSecurityUserAsync(uint mapId, uint userId)
    {
      try
      {
        Maps map = dbContext.Maps.Find(mapId);

        if (map == null)
          throw new OLabObjectNotFoundException(Utils.Constants.ScopeLevelMap, mapId);

        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        // only allow users with write-access to list map security users
        if (!auth.HasAccess("W", Utils.Constants.ScopeLevelMap, map.Id))
          throw new OLabUnauthorizedException(Utils.Constants.ScopeLevelMap, map.Id);

        var result = await _endpoint.UnsetMapSecurityUserAsync(map, userId);

        return OLabObjectResult<bool>.Result(result);
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
    /// <param name="dto"></param>
    private void DecorateDto(Dto.Designer.ScopedObjectsDto dto)
    {
      Type t = typeof(QuestionsController);
      var attribute =
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

      foreach (Dto.Designer.ScopedObjectDto item in dto.Questions)
        item.Url = $"{BaseUrl}/{questionRoute}/{item.Id}";

      foreach (Dto.Designer.ScopedObjectDto item in dto.Counters)
        item.Url = $"{BaseUrl}/{counterRoute}/{item.Id}";

      foreach (Dto.Designer.ScopedObjectDto item in dto.Constants)
        item.Url = $"{BaseUrl}/{constantRoute}/{item.Id}";

      foreach (Dto.Designer.ScopedObjectDto item in dto.Files)
        item.Url = $"{BaseUrl}/{fileRoute}/{item.Id}";
    }
  }
}
