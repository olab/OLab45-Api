using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Maps;

public partial class MapsFunction : OLabFunction
{
  private readonly Api.Endpoints.Player.MapsEndpoint _playerEndpoint;
  private readonly Api.Endpoints.Designer.MapsEndpoint _designerEndpoint;

  public MapsFunction(
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
  /// Get a pageable list of maps
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "MapsGet" )]
  public async Task<IActionResult> MapsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      var queryTake = Convert.ToInt32( request.Query[ "take" ] );
      var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
      int? take = queryTake > 0 ? queryTake : null;
      int? skip = querySkip > 0 ? querySkip : null;

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var result = await _playerEndpoint.GetAsync( auth, take, skip );
      Logger.LogInformation( string.Format( "Found {0} maps", result.Data.Count ) );

      return request
        .CreateResponse( OLabObjectPagedListResult<MapsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapsGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Gets the short status information for a map
  /// </summary>
  /// <param name="id">Map Id</param>
  /// <returns>MapStatusDto</returns>
  [Function( "MapGetShortStatus" )]
  public async Task<IActionResult> MapGetShortStatusAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/shortstatus" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _playerEndpoint.GetStatusAbbreviatedAsync( auth, id, cancellationToken );

      return request
        .CreateResponse( OLabObjectResult<MapStatusDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapGetShortStatusAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Gets the full status information for a map
  /// </summary>
  /// <param name="id">Map Id</param>
  /// <returns>MapStatusDto</returns>
  [Function( "MapStatusGet" )]
  public async Task<IActionResult> MapStatusGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/status" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _playerEndpoint.GetStatusAsync( auth, id, cancellationToken );
      return request
        .CreateResponse( OLabObjectResult<MapStatusDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapStatusGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "MapFullGet" )]
  public async Task<IActionResult> MapFullGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{id}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _playerEndpoint.GetAsync( auth, id );
      return request
        .CreateResponse( OLabObjectResult<MapsFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapFullGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Append template to an existing map
  /// </summary  
  /// <param name="mapId">Map to add template to</param>
  /// <param name="CreateMapRequest.templateId">Template to add to map</param>
  /// <returns>IActionResult</returns>
  [Function( "MapFullPut" )]
  public async Task<IActionResult> MapFullPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint mapId
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var body = await request.ParseBodyFromRequestAsync<MapsFullDto>();

      await _playerEndpoint.PutAsync( auth, mapId, body );
      return request
        .CreateResponse( OLabObjectResult<MapsFullDto>.Result( body ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapFullPut" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Create new map (using optional template)
  /// </summary>
  /// <param name="body">Create map request body</param>
  /// <returns>IActionResult</returns>
  [Function( "MapFullRelationsPost" )]
  public async Task<IActionResult> MapFullRelationsPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "maps" )] HttpRequestData request,
    FunctionContext executionContext
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var body = await request.ParseBodyFromRequestAsync<CreateMapRequest>();

      var dto = await _playerEndpoint.CreateMapAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<MapsFullRelationsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapFullRelationsPost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Gets the links for a map
  /// </summary>
  /// <param name="mapId"></param>
  /// <returns>MapNodeLinks dto</returns>
  [Function( "MapLinksGet" )]
  public async Task<IActionResult> MapLinksGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/links" )] HttpRequestData request,
    FunctionContext executionContext, CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _playerEndpoint.GetLinksAsync( auth, id );
      Logger.LogInformation( string.Format( "Found {0} map links", dto.Count ) );

      return request
        .CreateResponse( OLabObjectListResult<MapNodeLinksFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapLinksGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Gets the full status information for a map
  /// </summary>
  /// <param name="id">Map Id</param>
  /// <returns>MapStatusDto</returns>
  [Function( "MapDelete" )]
  public async Task<IActionResult> MapDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "maps/{id}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      await _playerEndpoint.DeleteMapAsync( auth, id );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

}
