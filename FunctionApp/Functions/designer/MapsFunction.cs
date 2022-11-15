using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLabWebAPI.Endpoints.Designer;

namespace OLab.Endpoints.Azure.Designer
{
  public class MapsFunction : OLabFunction
  {
    private readonly MapsEndpoint _endpoint;

    public MapsFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new MapsEndpoint(this.logger, context);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("MapNodeGetDesigner")]
    public async Task<IActionResult> MapNodeGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "designer/maps/{mapId}/node/{nodeId}")] HttpRequest request,
      uint mapId,
      uint nodeId,
      CancellationToken cancellationToken)
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);
        return OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        if (ex is System.ArgumentOutOfRangeException)
          return OLabBadRequestObjectResult.Result();
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get non-rendered nodes for a map
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [FunctionName("MapNodesGetDesigner")]
    public async Task<IActionResult> MapNodesGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "designer/maps/{mapId}/nodes")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dtoList = await _endpoint.GetMapNodesAsync(auth, mapId);
        return OLabObjectListResult<MapNodesFullDto>.Result(dtoList);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Create new node link
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [FunctionName("MapNodeLinkPostDesigner")]
    public async Task<IActionResult> MapNodeLinkPostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "designer/maps/{mapId}/nodes/{nodeId}/links")] HttpRequest request,
      uint mapId,
      uint nodeId
      )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        PostNewLinkRequest body = JsonConvert.DeserializeObject<PostNewLinkRequest>(content);

        var dto = await _endpoint.PostMapNodeLinkAsync(auth, mapId, nodeId, body);
        return OLabObjectResult<PostNewLinkResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Create new node
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [FunctionName("MapNodePostDesigner")]
    public async Task<IActionResult> MapNodePostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "designer/maps/{mapId}/nodes")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        PostNewNodeRequest body = JsonConvert.DeserializeObject<PostNewNodeRequest>(content);

        var dto = await _endpoint.PostMapNodesAsync(auth, body);
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
    /// Get raw scoped objects for map
    /// </summary>
    /// <param name="mapId">Map Id</param>
    /// <returns></returns>
    [FunctionName("MapScopedObjectsRawDesigner")]
    public async Task<IActionResult> MapScopedObjectsRawDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "designer/maps/{mapId}/scopedobjects/raw")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, mapId);
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
    [FunctionName("MapScopedObjectsDesigner")]
    public async Task<IActionResult> MapScopedObjectsDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "designer/maps/{mapId}/scopedobjects")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsAsync(auth, mapId);
        return OLabObjectResult<OLabWebAPI.Dto.Designer.ScopedObjectsDto>.Result(dto);
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
