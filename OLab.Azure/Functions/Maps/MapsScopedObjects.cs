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

namespace OLab.Azure.Functions.Maps;

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

      var dto = await _playerEndpoint.GetScopedObjectsRawAsync(auth, id);
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

      var dto = await _playerEndpoint.GetScopedObjectsAsync(auth, id);
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

  /// <summary>
  /// Get raw scoped objects for map
  /// </summary>
  /// <param name="mapId">Map Id</param>
  /// <returns></returns>
  [Function( "MapScopedObjectsRawDesignerGet" )]
  public async Task<IActionResult> MapScopedObjectsRawDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects/raw" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId
  )
  {
    try
    {
      Guard.Argument( mapId, nameof( mapId ) ).NotZero();
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _designerEndpoint.GetScopedObjectsRawAsync( auth, mapId );
      return request
        .CreateResponse( OLabObjectResult<Api.Dto.Designer.ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapScopedObjectsRawDesignerGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapScopedObjectsDesignerGet" )]
  public async Task<IActionResult> MapScopedObjectsDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/scopedobjects" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId
  )
  {
    try
    {
      Guard.Argument( mapId, nameof( mapId ) ).NotZero();
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _designerEndpoint.GetScopedObjectsAsync( auth, mapId );
      return request
        .CreateResponse( OLabObjectResult<Api.Dto.Designer.ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapScopedObjectsDesignerGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }
}
