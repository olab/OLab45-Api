using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Azure.Extensions;


namespace OLab.Azure.Functions.Maps;

public partial class MapNodesFunction : OLabFunction
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="sinceTime"></param>
  /// <returns></returns>
  [Function( "NodeDynamicObjectsRawGet" )]
  public async Task<IActionResult> NodeDynamicObjectsRawGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects/raw" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
    )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( mapId, nameof( mapId ) ).NotZero();
      Guard.Argument( nodeId, nameof( nodeId ) ).NotZero();

      uint sinceTime = 0;
      var sinceTimeQueryString = hostContext.BindingContext
                     .BindingData[ "sinceTime" ]
                     .ToString();
      if ( !string.IsNullOrEmpty( sinceTimeQueryString ) )
        sinceTime = (uint)Convert.ToInt32( sinceTimeQueryString );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _playerEndpoint.GetDynamicScopedObjectsRawAsync( auth, mapId, nodeId, sinceTime );
      return request
        .CreateResponse( OLabObjectResult<DynamicScopedObjectsDto>.Result( dto ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodeDynamicObjectsRawGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="sinceTime"></param>
  /// <returns></returns>
  [Function( "NodeDynamicObjectsGet" )]
  public async Task<IActionResult> NodeDynamicObjectsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( mapId, nameof( mapId ) ).NotZero();

      uint sinceTime = 0;
      var sinceTimeQueryString = hostContext.BindingContext
                     .BindingData[ "sinceTime" ]
                     .ToString();
      if ( !string.IsNullOrEmpty( sinceTimeQueryString ) )
        sinceTime = (uint)Convert.ToInt32( sinceTimeQueryString );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _playerEndpoint.GetDynamicScopedObjectsTranslatedAsync( auth, mapId, nodeId, sinceTime );
      return request
        .CreateResponse( OLabObjectResult<DynamicScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodeDynamicObjectsGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

}
