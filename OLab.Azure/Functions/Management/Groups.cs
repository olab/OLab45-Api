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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public partial class GroupsFunction : OLabFunction
{
  private readonly GroupsEndpoint _endpoint;

  public GroupsFunction(
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

    _endpoint = new GroupsEndpoint(
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
  [Function( "GroupGet" )]
  public async Task<IActionResult> GroupGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "groups/{source}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    string source)
  {
    try
    {
      Logger.LogInformation( $"GroupGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var dto = await _endpoint.GetAsync( auth, source );
      return request
        .CreateResponse( OLabObjectResult<GroupsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupGetAsync ) );
    }

  }

  /// <summary>
  /// ReadAsync a list of groups
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "GroupsGet" )]
  public async Task<IActionResult> GroupsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "groups" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupsGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<GroupsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupsGetAsync ) );
    }

  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "GroupPost" )]
  public async Task<IActionResult> GroupPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "groups/{name}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel,
    string name)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"GroupPostAsync" );

      var body = await request.ParseBodyFromRequestAsync<GroupsDto>( GetLogger() );
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.PostAsync( auth, name, cancel );
      return request
        .CreateResponse( OLabObjectResult<GroupsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupPostAsync ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "GroupDelete" )]
  public async Task<IActionResult> GroupDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "groups/{source}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    string source)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"GroupDelete" );

      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, source );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupDeleteAsync ) );
    }

  }

}
