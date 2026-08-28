using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Management;

public partial class RolesFunction : OLabFunction
{
  private readonly RolesEndpoint _endpoint;

  public RolesFunction(
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

    _endpoint = new RolesEndpoint(
      Logger,
      configuration,
      DbContext,
      _wikiTagProvider,
      _fileStorageProvider );
  }

  /// <summary>
  /// Get single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "RoleGet" )]
  public async Task<HttpResponseData> RoleGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "roles/{source}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    string source)
  {
    try
    {
      Logger.LogInformation( $"RoleGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _endpoint.GetAsync( auth, source );
      return request
        .CreateResponse( OLabObjectResult<RolesDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( RoleGetAsync ) );
    }

  }

  /// <summary>
  /// ReadAsync a list of Roles
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "RolesGet" )]
  public async Task<HttpResponseData> RolesGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "roles" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"RolesGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var pagedResponse = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<RolesDto>.Result( pagedResponse.Data, pagedResponse.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( RolesGetAsync ) );
    }

  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "RolePost" )]
  public async Task<HttpResponseData> RolePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "roles/{name}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel,
    string name)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"RolePostAsync" );

      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<RolesDto>(GetLogger() );

      var dto = await _endpoint.PostAsync( auth, name, cancel );
      return request
        .CreateResponse( OLabObjectResult<RolesDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( RolePostAsync ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "RoleDelete" )]
  public async Task<HttpResponseData> RoleDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "roles/{source}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    string source)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"RoleDelete" );

      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, source );
      return OLabFunctionResponses.OLabNoContentResponse( request );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( RoleDeleteAsync ) );
    }

  }

}
