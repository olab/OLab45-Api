using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
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
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagModules) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(wikiTagModules).NotNull(nameof(wikiTagModules));

      Logger = OLabLogger.CreateNew<MapsFunction>(loggerFactory);

      _endpoint = new MapsEndpoint(Logger, configuration, dbContext, wikiTagModules);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("MapNodeGetDesigner")]
    public async Task<HttpResponseData> MapNodeGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/node/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint mapId,
      uint nodeId)
    {
      try
      {
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetMapNodeAsync(auth, mapId, nodeId);
        response = request.CreateResponse(OLabObjectResult<MapsNodesFullRelationsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Get non-rendered nodes for a map
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [Function("MapNodesGetDesigner")]
    public async Task<HttpResponseData> MapNodesGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/nodes")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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
        response = request.CreateResponse(OLabObjectListResult<MapNodesFullDto>.Result(dtoList));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Create new node link
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [Function("MapNodeLinkPostDesigner")]
    public async Task<HttpResponseData> MapNodeLinkPostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "designer/maps/{mapId}/nodes/{nodeId}/links")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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

        response = request.CreateResponse(OLabObjectResult<PostNewLinkResponse>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }

    /// <summary>
    /// Create new node
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [Function("MapNodePostDesigner")]
    public async Task<HttpResponseData> MapNodePostDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "designer/maps/{mapId}/nodes")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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

        response = request.CreateResponse(OLabObjectResult<PostNewNodeResponse>.Result(dto));

      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Get raw scoped objects for map
    /// </summary>
    /// <param name="mapId">Map Id</param>
    /// <returns></returns>
    [Function("MapScopedObjectsRawDesigner")]
    public async Task<HttpResponseData> MapScopedObjectsRawDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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
        response = request.CreateResponse(OLabObjectResult<OLab.Api.Dto.Designer.ScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapScopedObjectsDesigner")]
    public async Task<HttpResponseData> MapScopedObjectsDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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
        response = request.CreateResponse(OLabObjectResult<Api.Dto.Designer.ScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }
  }
}
