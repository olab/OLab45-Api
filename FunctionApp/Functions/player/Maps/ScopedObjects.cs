using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class MapsFunction : OLabFunction
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapScopedObjectsRawGet")]
    public async Task<IActionResult> MapScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
        return OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto);
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
    [Function("MapScopedObjectsGet")]
    public async Task<IActionResult> MapScopedObjectsPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
        return OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto);
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
