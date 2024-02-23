using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Utils;
using OLab.FunctionApp.Extensions;
using OLab.FunctionApp.Functions.API;

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
    public async Task<HttpResponseData> MapScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
        response = request.CreateResponse(OLabObjectResult<Api.Dto.ScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
        response = request.CreateResponse(ex);
      }

      return response;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapScopedObjectsGet")]
    public async Task<HttpResponseData> MapScopedObjectsGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
        response = request.CreateResponse(OLabObjectResult<Api.Dto.ScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
        response = request.CreateResponse(ex);
      }

      return response;
    }
  }
}
