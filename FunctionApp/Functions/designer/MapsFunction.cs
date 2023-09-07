using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Microsoft.Extensions.Logging;

using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Data.Exceptions;
using OLab.Api.Utils;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Designer
{
  public class MapsFunction : OLabFunction
  {
    private readonly MapsEndpoint _endpoint;

    public MapsFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new MapsEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("MapNodeGetDesigner")]
    public async Task<IActionResult> MapNodeGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/node/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId,
      uint nodeId,
      CancellationToken cancellationToken)
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
    [Function("MapNodesGetDesigner")]
    public async Task<IActionResult> MapNodesGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/nodes")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
    [Function("MapNodeLinkPostDesigner")]
    public async Task<IActionResult> MapNodeLinkPostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "designer/maps/{mapId}/nodes/{nodeId}/links")] HttpRequestData request,
      FunctionContext hostContext,
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
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<PostNewLinkRequest>();
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
    [Function("MapNodePostDesigner")]
    public async Task<IActionResult> MapNodePostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "designer/maps/{mapId}/nodes")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<PostNewNodeRequest>();
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
    [Function("MapScopedObjectsRawDesigner")]
    public async Task<IActionResult> MapScopedObjectsRawDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, mapId);
        return OLabObjectResult<OLab.Api.Dto.Designer.ScopedObjectsDto>.Result(dto);
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
    [Function("MapScopedObjectsDesigner")]
    public async Task<IActionResult> MapScopedObjectsDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId
    )
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsAsync(auth, mapId);
        return OLabObjectResult<Api.Dto.Designer.ScopedObjectsDto>.Result(dto);
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
