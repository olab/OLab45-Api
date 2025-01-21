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
using OLab.Azure.Functions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;


namespace OLab.Azure.Functions.Player.Maps.Links;

public partial class LinksFunction : OLabFunction
{
  private readonly MapsEndpoint _endpoint;

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

    _endpoint = new MapsEndpoint(
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
  [HttpPut( "{mapId}/nodes/{nodeId}/links/{linkId}" )]
  public async Task<IActionResult> PutMapNodeLinksAsync(
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

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<MapNodeLinksFullDto>();

      await _endpoint.PutMapNodeLinksAsync( auth, mapId, nodeId, linkId, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodePostAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }
}
