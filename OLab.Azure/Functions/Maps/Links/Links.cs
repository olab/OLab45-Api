using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Maps.Links;

public partial class LinksFunction : OLabFunction
{
  private readonly Api.Endpoints.Player.MapsEndpoint _playerEndpoint;
  private readonly Api.Endpoints.Designer.MapsEndpoint _designerEndpoint;

  public LinksFunction(
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

    Logger = OLabLogger.CreateNew<LinksFunction>( loggerFactory );

    _playerEndpoint = new Api.Endpoints.Player.MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );

    _designerEndpoint = new Api.Endpoints.Designer.MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Saves a link edit
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <param name="linkId">link id</param>
  /// <returns>IActionResult</returns>
  [Function( "MapNodeLinkPut" )]
  public async Task<HttpResponseData> MapNodeLinkPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}/links/{linkId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId,
    uint linkId
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"MapNodeLinkPutAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<MapNodeLinksFullDto>( GetLogger() );

      await _playerEndpoint.PutMapNodeLinksAsync( auth, mapId, nodeId, linkId, body );
      return OLabFunctionResponses.OLabNoContentResponse( request );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( MapNodeLinkPutAsync ) );
    }

  }

  /// <summary>
  /// Create new node link
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns>IActionResult</returns>
  [Function( "MapNodeLinkDesignerPost" )]
  public async Task<HttpResponseData> MapNodeLinkPostDesignerAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "designer/maps/{mapId}/nodes/{nodeId}/links" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId
    )
  {
    try
    {
      Guard.Argument( mapId, nameof( mapId ) ).NotZero();
      Guard.Argument( nodeId, nameof( nodeId ) ).NotZero();
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"MapNodeLinkPostDesignerAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<PostNewLinkRequest>( GetLogger() );

      var dto = await _designerEndpoint.PostMapNodeLinkAsync( auth, mapId, nodeId, body );
      return request
        .CreateResponse( OLabObjectResult<PostNewLinkResponse>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( MapNodeLinkPostDesignerAsync ) );
    }

  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapNodeLinkDesignerDelete" )]
  public async Task<HttpResponseData> MapNodeLinkDesignerDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "designer/maps/{mapId}/links/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      var auth = GetAuthorization( hostContext );

      await _designerEndpoint.DeleteMapNodeLinkAsync( auth, mapId, id );
      return OLabFunctionResponses.OLabNoContentResponse( request );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( MapNodeLinkDesignerDeleteAsync ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapNodeLinkDesignerGet" )]
  public async Task<HttpResponseData> MapNodeLinkDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "designer/maps/{mapId}/nodes/{nodeId}/links/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      var auth = GetAuthorization( hostContext );

      var dto = await _designerEndpoint.GetMapNodeLinkAsync( auth, mapId, id );
      return request
        .CreateResponse( OLabObjectResult<MapNodeLinksDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( MapNodeLinkDesignerGetAsync ) );
    }
  }
}
