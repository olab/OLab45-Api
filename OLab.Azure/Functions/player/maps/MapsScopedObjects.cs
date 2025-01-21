using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Azure.Extensions;
using OLab.Azure.Functions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Player.Maps;

public partial class MapsFunction : OLabFunction
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function("MapGetScopedObjectsRaw")]
  public async Task<IActionResult> MapGetScopedObjectsRawAsync(
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
      return request
        .CreateResponse( OLabObjectResult<ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapGetScopedObjectsRaw" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function("MapScopedObjectsGet")]
  public async Task<IActionResult> MapScopedObjectsGetAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
  uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
      return request
        .CreateResponse( OLabObjectResult<ScopedObjectsDto>.Result( dto ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodePostAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }
}
