using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.FunctionApp.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.Player;

public partial class NodesFunction : OLabFunction
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function("MapNodeScopedObjectsRawGet")]
  public async Task<HttpResponseData> MapScopedObjectsRawGetAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects/raw")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
      response = request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
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
  [Function("MapNodeScopedObjectsGet")]
  public async Task<HttpResponseData> MapScopedObjectsGetAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
      response = request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }
}
