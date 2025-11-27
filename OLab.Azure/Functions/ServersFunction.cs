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

public partial class ServersFunction : OLabFunction
{
  private readonly ServerEndpoint _endpoint;

  public ServersFunction(
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

    Logger = OLabLogger.CreateNew<ServersFunction>( loggerFactory );

    _endpoint = new ServerEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider );
  }

  /// <summary>
  /// Read a list of object
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "ServersGet" )]
  public async Task<IActionResult> GetServersAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "servers" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"ServersGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var result = await _endpoint.GetPhysAsync<Servers>( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<Servers>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GetServersAsync ) );
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
      return ProcessException( request, ex, nameof( ServerScopedObjectRawGetAsync ) );
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
      return ProcessException( request, ex, nameof( ServerScopedObjectGetAsync ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="serverId"></param>
  /// <returns></returns>
  [Function( "ServerDynamicObjectGet" )]
  public async Task<IActionResult> ServerDynamicObjectGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "servers/{id}/dynamicobjects" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _endpoint.GetDynamicObjectsTranslatedAsync( id );
      return request
        .CreateResponse( OLabObjectResult<ScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ServerDynamicObjectGetAsync ) );
    }
  }
}
