using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Dto.Designer;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Maps;

public class TemplateFunction : OLabFunction
{
  private readonly TemplateEndpoint _endpoint;

  public TemplateFunction(
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

    Logger = OLabLogger.CreateNew<TemplateFunction>( loggerFactory );
    _endpoint = new TemplateEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Gets map node
  /// </summary>
  /// <param name="request"></param>
  /// <param name="logger"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  [Function( "TemplateDesignerGet" )]
  public async Task<IActionResult> TemplateDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "templates" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"TemplateDesignerGet" );

      var queryTake = Convert.ToInt32( request.Query[ "take" ] );
      var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
      int? take = queryTake > 0 ? queryTake : null;
      int? skip = querySkip > 0 ? querySkip : null;

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var result = await _endpoint.GetAsync( take, skip );
      Logger.LogInformation( string.Format( "Found {0} files", result.Data.Count ) );

      return request
        .CreateResponse( OLabObjectPagedListResult<MapsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "TemplateDesignerGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Get links for templates
  /// </summary>
  /// <param name="id">Constant id</param>
  /// <returns></returns>
  [Function( "TemplateLinksDesignerGet" )]
  public IActionResult TemplateLinksDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "templates/links" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"TemplateLinksDesignerGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = _endpoint.Links();
      return request
        .CreateResponse( OLabObjectResult<MapNodeLinkTemplateDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "TemplateLinksDesignerGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Get template nodes
  /// </summary>
  /// <param name="id">Constant id</param>
  /// <returns></returns>
  [Function( "TemplateMapNodeDesignerGet" )]
  public IActionResult TemplateMapNodeDesignerGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "templates/nodes" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"TemplateMapNodeDesignerGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = _endpoint.Nodes();
      return request
        .CreateResponse( OLabObjectResult<MapNodeTemplateDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "TemplateMapNodeDesignerGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
