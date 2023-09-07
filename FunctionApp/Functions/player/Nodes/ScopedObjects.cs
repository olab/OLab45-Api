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
  public partial class NodesFunction : OLabFunction
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapNodeScopedObjectsRawGet")]
    public async Task<IActionResult> MapScopedObjectsRawGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext,
      uint nodeId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
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
    [Function("MapNodeScopedObjectsGet")]
    public async Task<IActionResult> MapScopedObjectsGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext,
      uint nodeId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
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
