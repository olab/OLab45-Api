using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class NodesFunction : OLabFunction
  {
    private readonly NodesEndpoint _endpoint;

    public NodesFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new NodesEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// Get full map node, with relations
    /// </summary>
    /// <param name="nodeId">Node id (0, if root node)</param>
    /// <returns>MapsNodesFullRelationsDto response</returns>
    [Function("MapNodeGetPlayer")]
    public async Task<IActionResult> MapNodeGetPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint nodeId)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);
        var dto = await _endpoint.GetNodeTranslatedAsync(auth, nodeId);

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
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Function("MapNodePutPlayer")]
    public async Task<IActionResult> MapNodePutPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "nodes/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);
        var body = await request.ParseBodyFromRequestAsync<MapNodesFullDto>();

        await _endpoint.PutNodeAsync(auth, id, body);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return new NoContentResult();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [Function("MapNodeLinkPostPlayer")]
    public async Task<IActionResult> MapNodeLinkPostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "nodes/{nodeId}/links")] HttpRequestData request,
      FunctionContext hostContext,
      uint nodeId
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<MapNodeLinksPostDataDto>();
        var dto = await _endpoint.PostLinkAsync(auth, nodeId, body);

        return OLabObjectResult<MapNodeLinksPostResponseDto>.Result(dto);
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
    /// <param name="mapId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [Function("MapNodePostPlayer")]
    public async Task<IActionResult> MapNodePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "nodes/{mapId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint mapId
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<MapNodesPostDataDto>();
        var dto = await _endpoint.PostNodeAsync(auth, mapId, body);

        return OLabObjectResult<MapNodesPostResponseDto>.Result(dto);
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
