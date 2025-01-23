using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Azure.Extensions;

namespace OLab.Azure.Functions.Player;

public partial class NodesFunction : OLabFunction
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapNodeScopedObjectsRawGet" )]
  public async Task<IActionResult> MapScopedObjectsRawGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects/raw" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetScopedObjectsAsync( nodeId, false );
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
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapNodeScopedObjectsGet" )]
  public async Task<IActionResult> MapScopedObjectsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}/scopedobjects" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetScopedObjectsAsync( nodeId, true );
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
