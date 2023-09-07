using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;

namespace OLab.FunctionApp.Functions.Player
{
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
    public async Task<IActionResult> MapNodeDynamicScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects/raw")] HttpRequestData request,
      FunctionContext hostContext,
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
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [Function("MapNodeDynamicScopedObjects")]
    public async Task<IActionResult> MapNodeDynamicScopedObjectsAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects")] HttpRequestData request,
      FunctionContext hostContext,
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
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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
