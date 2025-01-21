using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Azure.Functions.Player.Maps;

public partial class MapNodesFunction : OLabFunction
{
  private readonly MapsEndpoint _endpoint;

  public MapNodesFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider )

  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<MapsFunction>( loggerFactory );

    _endpoint = new MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );

  }

  /// <summary>
  /// Plays specific map node
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <returns>IActionResult</returns>
  [Function( "MapNodePost" )]
  public async Task<IActionResult> MapNodePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "maps/{mapId}/node/{nodeId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<DynamicScopedObjectsDto>();

      var dto = await _endpoint.PlayMapNodeAsync( auth, mapId, nodeId, body );
      return request
        .CreateResponse( OLabObjectResult<MapsNodesFullRelationsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapNodePost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Delete a node from the map
  /// </summary>
  /// <param name="mapId">map id that owns node</param>
  /// <param name="nodeId">node id</param>
  /// <returns>IActionResult</returns>
  [Function( "MapNodeDelete" )]
  public async Task<IActionResult> MapNodeDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "maps/{mapId}/nodes/{nodeId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.DeleteNodeAsync( auth, mapId, nodeId );
      return request
        .CreateResponse( OLabObjectResult<MapNodesPostResponseDto>.Result( dto ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapNodeDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Updates specific map node
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <param name="dto">node data</param>
  /// <returns>IActionResult</returns>
  [Function( "MapNodePut" )]
  public async Task<IActionResult> MapNodePutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<MapNodesFullDto>();

      var dto = await _endpoint.PutNodeAsync( auth, mapId, nodeId, body );
      return request
        .CreateResponse( OLabObjectResult<MapNodesPostResponseDto>.Result( dto ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapNodePut" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

}
