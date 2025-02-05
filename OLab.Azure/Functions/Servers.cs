using Dawn;
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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public partial class Servers : OLabFunction
{
  private readonly ServerEndpoint _endpoint;

  public Servers(
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
    Guard.Argument( wikiTagProvider ).NotNull( nameof( wikiTagProvider ) );
    Guard.Argument( fileStorageProvider ).NotNull( nameof( fileStorageProvider ) );

    Logger = OLabLogger.CreateNew<Servers>( loggerFactory );

    _endpoint = new ServerEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider );
  }

  /// <summary>
  /// ReadAsync a list of servers
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "ServersGet" )]
  public async Task<IActionResult> ServersGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "servers" )] HttpRequestData request,
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

      var pagedResponse = await _endpoint.GetAsync( take, skip );
      return request
        .CreateResponse( OLabObjectListResult<Api.Model.Servers>.Result( pagedResponse.Data ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "ServersGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="serverId"></param>
  /// <returns></returns>
  [Function( "ServerScopedObjectRawGet" )]
  public async Task<IActionResult> ServerScopedObjectRawGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "servers/{id}/scopedobjects/raw" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _endpoint.GetScopedObjectsRawAsync( id );
      return request
        .CreateResponse( OLabObjectResult<ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "ServerScopedObjectRawGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="serverId"></param>
  /// <returns></returns>
  [Function( "ServerScopedObjectGet" )]
  public async Task<IActionResult> ServerScopedObjectGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "servers/{id}/scopedobjects" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _endpoint.GetScopedObjectsTranslatedAsync( id );
      return request
        .CreateResponse( OLabObjectResult<ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "ServerScopedObjectGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }
}
