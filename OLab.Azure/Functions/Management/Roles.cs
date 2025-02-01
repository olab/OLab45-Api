using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Azure.Functions;

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

    Logger = OLabLogger.CreateNew<Servers>( loggerFactory );

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
  public async Task<IActionResult> GroupGetAsync(
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
      Logger.LogError( ex, "RoleGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// ReadAsync a list of Roles
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "RolesGet" )]
  public async Task<IActionResult> RolesGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "roles" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      var queryTake = Convert.ToInt32( request.Query[ "take" ] );
      var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
      int? take = queryTake > 0 ? queryTake : null;
      int? skip = querySkip > 0 ? querySkip : null;

      Logger.LogInformation( $"RolesGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var pagedResponse = await _endpoint.GetAsync( auth, take, skip );
      return request
        .CreateResponse( OLabObjectPagedListResult<RolesDto>.Result( pagedResponse.Data, pagedResponse.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "RolesGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "RolePost" )]
  public async Task<IActionResult> ConstantPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "roles/{name}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel,
    string name)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"RolePost" );

      var body = await request.ParseBodyFromRequestAsync<RolesDto>();
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.PostAsync( auth, name, cancel );
      return request
        .CreateResponse( OLabObjectResult<RolesDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "RolePost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "RoleDelete" )]
  public async Task<IActionResult> GroupDeleteAsync(
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
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "RoleDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
