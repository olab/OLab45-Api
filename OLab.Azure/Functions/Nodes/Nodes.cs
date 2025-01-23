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

namespace OLab.Azure.Functions.Player;

public partial class NodesFunction : OLabFunction
{
  private readonly NodesEndpoint _endpoint;

  public NodesFunction(
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

    Logger = OLabLogger.CreateNew<NodesFunction>( loggerFactory );
    _endpoint = new NodesEndpoint(
      Logger,
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Get full map node, with relations
  /// </summary>
  /// <param name="nodeId">Node id (0, if root node)</param>
  /// <returns>MapsNodesFullRelationsDto response</returns>
  [Function( "NodeGet" )]
  public async Task<IActionResult> NodeGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "nodes/{nodeId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetNodeTranslatedAsync( auth, nodeId );
      return request
        .CreateResponse( OLabObjectResult<MapsNodesFullRelationsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodeGetAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <param name="dto"></param>
  /// <returns></returns>
  [Function( "NodePut" )]
  public async Task<IActionResult> NodePutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "nodes/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<MapNodesFullDto>();

      await _endpoint.PutNodeAsync( auth, id, body );
      response = request.CreateNoContentResponse();
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodePut" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="nodeId"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  [Function( "NodePostLinks" )]
  public async Task<IActionResult> NodePostLinksAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "nodes/{nodeId}/links" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint nodeId
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<MapNodeLinksPostDataDto>();

      var dto = await _endpoint.PostLinkAsync( auth, nodeId, body );
      return request
        .CreateResponse( OLabObjectResult<MapNodeLinksPostResponseDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodePostLinks" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapId"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  [Function( "NodePost" )]
  public async Task<IActionResult> NodePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "nodes/{mapId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId
  )
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<MapNodesPostDataDto>();

      var dto = await _endpoint.PostNodeAsync( auth, mapId, body );
      return request
        .CreateResponse( OLabObjectResult<MapNodesPostResponseDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "NodePost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

}
