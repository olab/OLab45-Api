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
using OLabWebAPI.Endpoints.Player;

namespace OLab.Endpoints.Azure.Player
{
  public partial class NodesAzureEndpoint : OLabAzureEndpoint
  {
    private readonly NodesEndpoint _endpoint;

    public NodesAzureEndpoint(
      IUserService userService,
      ILogger<ConstantsAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new NodesEndpoint(this.logger, context);
    }

    /// <summary>
    /// Get full map node, with relations
    /// </summary>
    /// <param name="nodeId">Node id (0, if root node)</param>
    /// <returns>MapsNodesFullRelationsDto response</returns>
    [FunctionName("MapNodeGetPlayer")]
    public async Task<IActionResult> MapNodeGetPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "nodes/{nodeId}")] HttpRequest request,
      uint nodeId)
    {
      try
      {
        var dto = await _endpoint.GetNodeTranslatedAsync(nodeId);
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
    [FunctionName("MapNodePutPlayer")]
    public async Task<IActionResult> MapNodePutPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "put", Route = "nodes/{nodeId}")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        MapNodesFullDto body = JsonConvert.DeserializeObject<MapNodesFullDto>(content);

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
    [FunctionName("MapNodeLinkPostPlayer")]
    public async Task<IActionResult> MapNodeLinkPostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "nodes/{nodeId}/links")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        MapNodeLinksPostDataDto body = JsonConvert.DeserializeObject<MapNodeLinksPostDataDto>(content);

        var dto = await _endpoint.PostLinkAsync(mapId, nodeId, body);
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
    [FunctionName("MapNodePostPlayer")]
    public async Task<IActionResult> MapNodePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "nodes/{nodeId}")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        MapNodesPostDataDto body = JsonConvert.DeserializeObject<MapNodesPostDataDto>(content);

        var dto = await _endpoint.PostNodeAsync(mapId, body);
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
