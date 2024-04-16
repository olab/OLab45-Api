using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Endpoints.WebApi.Player;
using OLabWebAPI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Designer;

[Route("olab/api/v3/designer/maps")]
[ApiController]
public partial class MapsController : OLabController
{
  private readonly MapsEndpoint _endpoint;

  public MapsController(
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

    Logger = OLabLogger.CreateNew<MapsController>(loggerFactory);

    _endpoint = new MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider);
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
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      //Guard.Argument(nodeId, nameof(nodeId)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// ReadAsync non-rendered nodes for a map
  /// </summary>
  /// <param name="mapId">Map id</param>
  /// <returns>IActionResult</returns>
  [HttpGet("{mapId}/nodes")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetMapNodesAsync(uint mapId)
  {
    try
    {
      Guard.Argument(mapId, nameof(mapId)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dtoList = await _endpoint.GetMapNodesAsync(auth, mapId);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<MapNodesFullDto>.Result(dtoList));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpDelete("{mapId}/nodes/{nodeId}/links/{id}")]
  public async Task<IActionResult> MapNodeLinkDeleteDesigner(
    uint mapId,
    uint nodeId,
    uint id,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(nodeId, nameof(nodeId)).NotZero();
      Guard.Argument(id, nameof(id)).NotZero();

      var auth = GetAuthorization(HttpContext);

      await _endpoint.DeleteMapNodeLinkAsync(auth, mapId, id);
      return NoContent();
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{mapId}/nodes/{nodeId}/links/{id}")]
  public async Task<IActionResult> MapNodeLinkGetDesigner(
    uint mapId,
    uint nodeId,
    uint id,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(nodeId, nameof(nodeId)).NotZero();
      Guard.Argument(id, nameof(id)).NotZero();

      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetMapNodeLinkAsync(auth, mapId, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapNodeLinksDto>.Result(dto));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Create a new node link
  /// </summary>
  /// <returns>IActionResult</returns>
  [HttpPost("{mapId}/nodes/{nodeId}/links")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostMapNodeLinkAsync(
    uint mapId,
    uint nodeId,
    [FromBody] PostNewLinkRequest body)
  {
    try
    {
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(nodeId, nameof(nodeId)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.PostMapNodeLinkAsync(auth, mapId, nodeId, body);
      return HttpContext.Request.CreateResponse(OLabObjectResult<PostNewLinkResponse>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(linkId, nameof(linkId)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var deleted = await _endpoint.DeleteMapNodeLinkAsync(auth, mapId, linkId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<bool>.Result(deleted));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      Guard.Argument(body).NotNull(nameof(body));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.PostMapNodesAsync(auth, body);
      return HttpContext.Request.CreateResponse(OLabObjectResult<PostNewNodeResponse>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(body).NotNull(nameof(body));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.PutMapNodegridAsync(auth, mapId, body);
      return HttpContext.Request.CreateResponse(OLabObjectResult<bool>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<OLab.Api.Dto.Designer.ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<OLab.Api.Dto.Designer.ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <param name="enableWikiTranslation"></param>
  /// <returns></returns>
  //private async Task<IActionResult> GetScopedObjectsAsync(
  //  uint id,
  //  bool enableWikiTranslation)
  //{
  //  try
  //  {
  //    Dto.Designer.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
  //    DecorateDto(dto);
  //    return HttpContext.Request.CreateResponse(OLabObjectResult<Dto.Designer.ScopedObjectsDto>.Result(dto));
  //  }
  //  catch (Exception ex)
  //  {
  //    if (ex is OLabUnauthorizedException)
  //      return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
  //    return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
  //  }
  //}

  /// <summary>
  /// ReadAsync a list of users
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
      Guard.Argument(mapId, nameof(mapId)).NotZero();

      var map = DbContext.Maps.Find(mapId);

      if (map == null)
        throw new OLabObjectNotFoundException(OLab.Api.Utils.Constants.ScopeLevelMap, mapId);

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      // only allow users with write-access to list olab users users from this endpoint
      if (!auth.HasAccess(SecurityRoles.Write, OLab.Api.Utils.Constants.ScopeLevelMap, map.Id))
        throw new OLabUnauthorizedException(OLab.Api.Utils.Constants.ScopeLevelMap, map.Id);

      var dtos = _endpoint.GetMapAccessCandidates(map, search ?? "");

      var list = new List<Hashtable>();

      foreach (var user in dtos)
      {
        list.Add(new Hashtable
        {
          { "id", user.Id },
          { "email", user.Email },
          { "username", user.Username },
          { "nickname", user.Nickname },
        });
      }

      return HttpContext.Request.CreateResponse(OLabObjectResult<IList<Hashtable>>.Result(list));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      var map = DbContext.Maps.Find(mapId);

      if (map == null)
        throw new OLabObjectNotFoundException(OLab.Api.Utils.Constants.ScopeLevelMap, mapId);

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      // only allow users with write-access to list olab users users from this endpoint
      if (!auth.HasAccess(SecurityRoles.Write, OLab.Api.Utils.Constants.ScopeLevelMap, map.Id))
        throw new OLabUnauthorizedException(OLab.Api.Utils.Constants.ScopeLevelMap, map.Id);

      body.MapId = map.Id;

      var result = await _endpoint.PutMapAccessCandidateAsync(map, body);

      return HttpContext.Request.CreateResponse(OLabObjectResult<int>.Result(result));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// ReadAsync a list of security users for a given map
  /// </summary>
  /// <param name="mapId"></param>
  /// <returns></returns>
  [HttpGet("{mapId}/securityusers")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public IActionResult GetSecurityUsers(uint mapId)
  {
    try
    {
      var map = DbContext.Maps.Find(mapId);

      if (map == null)
        throw new OLabObjectNotFoundException(OLab.Api.Utils.Constants.ScopeLevelMap, mapId);

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      // only allow users with write-access to list map security users
      if (!auth.HasAccess(SecurityRoles.Write, OLab.Api.Utils.Constants.ScopeLevelMap, map.Id))
        throw new OLabUnauthorizedException(OLab.Api.Utils.Constants.ScopeLevelMap, map.Id);

      var dtos = _endpoint.GetSecurityUsersRaw(map);

      var list = new List<Hashtable>();

      foreach (var rule in dtos)
      {
        var user = DbContext.Users.Where(x => x.Id == rule.UserId).FirstOrDefault();

        list.Add(new Hashtable
        {
          { "userId", rule.UserId },
          { "acl", rule.Acl2 },
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

      return HttpContext.Request.CreateResponse(OLabObjectResult<IList<Hashtable>>.Result(list));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      var map = DbContext.Maps.Find(mapId);

      if (map == null)
        throw new OLabObjectNotFoundException(OLab.Api.Utils.Constants.ScopeLevelMap, mapId);

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      // only allow users with write-access to list map security users
      if (!auth.HasAccess(SecurityRoles.Write, OLab.Api.Utils.Constants.ScopeLevelMap, map.Id))
        throw new OLabUnauthorizedException(OLab.Api.Utils.Constants.ScopeLevelMap, map.Id);

      var result = await _endpoint.SetMapSecurityUserAsync(map, body);

      return HttpContext.Request.CreateResponse(OLabObjectResult<bool>.Result(result));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      var map = DbContext.Maps.Find(mapId);

      if (map == null)
        throw new OLabObjectNotFoundException(OLab.Api.Utils.Constants.ScopeLevelMap, mapId);

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      // only allow users with write-access to list map security users
      if (!auth.HasAccess(SecurityRoles.Write, OLab.Api.Utils.Constants.ScopeLevelMap, map.Id))
        throw new OLabUnauthorizedException(OLab.Api.Utils.Constants.ScopeLevelMap, map.Id);

      var result = await _endpoint.UnsetMapSecurityUserAsync(map, userId);

      return HttpContext.Request.CreateResponse(OLabObjectResult<bool>.Result(result));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="dto"></param>
  private void DecorateDto(OLab.Api.Dto.Designer.ScopedObjectsDto dto)
  {
    var t = typeof(QuestionsController);
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
