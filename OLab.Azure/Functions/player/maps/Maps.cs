using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints.Player;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Dto;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace OLab.Azure.Functions.player.maps;

public partial class MapsFunction : OLabFunction
{
  private readonly MapsEndpoint _endpoint;

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

    _endpoint = new MapsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );

  }

  /// <summary>
  /// Get a list of maps
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "MapsGetPlayer" )]
  public async Task<IActionResult> MapsGetPlayerAsync(
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

      var pagedResult = await _endpoint.GetAsync( auth, take, skip );
      Logger.LogInformation( string.Format( "Found {0} maps", pagedResult.Data.Count ) );

      return request
        .CreateResponse( OLabObjectPagedListResult<MapsDto>.Result( pagedResult.Data, pagedResult.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapsGetPlayer" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
