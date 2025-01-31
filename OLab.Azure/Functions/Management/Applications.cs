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

public partial class ApplicationsFunction : OLabFunction
{
  private readonly ApplicationsEndpoint _endpoint;

  public ApplicationsFunction(
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

    _endpoint = new ApplicationsEndpoint(
      Logger,
      configuration,
      DbContext );
  }

  /// <summary>
  /// ReadAsync a list of Roles
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "ApplicationsGet" )]
  public async Task<IActionResult> ApplicationsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "applications" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      var queryTake = Convert.ToInt32( request.Query[ "take" ] );
      var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
      int? take = queryTake > 0 ? queryTake : null;
      int? skip = querySkip > 0 ? querySkip : null;

      Logger.LogDebug( $"ApplicationsGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );

      var pagedResponse = await _endpoint.GetAsync( auth, take, skip );
      return request
        .CreateResponse( OLabObjectPagedListResult<ApplicationsDto>.Result( pagedResponse.Data, pagedResponse.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "ApplicationsGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
