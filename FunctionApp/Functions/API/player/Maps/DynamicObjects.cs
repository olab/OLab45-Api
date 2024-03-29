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

public partial class MapsFunction : OLabFunction
{

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="sinceTime"></param>
  /// <returns></returns>
  [Function("MapNodeDynamicScopedObjectsRaw")]
  public async Task<HttpResponseData> MapNodeDynamicScopedObjectsRawAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects/raw")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
    )
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(mapId, nameof(mapId)).NotZero();
      Guard.Argument(nodeId, nameof(nodeId)).NotZero();

      uint sinceTime = 0;
      var sinceTimeQueryString = hostContext.BindingContext
                     .BindingData["sinceTime"]
                     .ToString();
      if (!string.IsNullOrEmpty(sinceTimeQueryString))
        sinceTime = (uint)Convert.ToInt32(sinceTimeQueryString);

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId, sinceTime);
      response = request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="sinceTime"></param>
  /// <returns></returns>
  [Function("MapNodeDynamicScopedObjects")]
  public async Task<HttpResponseData> MapNodeDynamicScopedObjectsAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(mapId, nameof(mapId)).NotZero();

      uint sinceTime = 0;
      var sinceTimeQueryString = hostContext.BindingContext
                     .BindingData["sinceTime"]
                     .ToString();
      if (!string.IsNullOrEmpty(sinceTimeQueryString))
        sinceTime = (uint)Convert.ToInt32(sinceTimeQueryString);

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(auth, mapId, nodeId, sinceTime);
      response = request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;
  }

}
