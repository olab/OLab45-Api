using Dawn;
using FluentValidation;
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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ScopedObjects;

public class Constants : OLabFunction
{
  private readonly ConstantsEndpoint _endpoint;

  public Constants(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<Constants>( loggerFactory );
    _endpoint = new ConstantsEndpoint( Logger, configuration, DbContext );
  }

  /// <summary>
  /// Gets all constants
  /// </summary>
  /// <param name="request"></param>
  /// <param name="logger"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  [Function( "ConstantsGet" )]
  public async Task<IActionResult> ConstantsGetAsync(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "constants" )] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken cancellationToken)
  {

    try
    {
      Logger.LogInformation( $"ConstantsGet" );

      var pageSpecs = ExtractPageParameters( request );

      var auth = GetAuthorization( hostContext );
      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<ConstantsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ConstantsGetAsync ) );
    }
  }

  /// <summary>
  /// Gets single constant
  /// </summary>
  /// <param name="id">Constant id</param>
  /// <returns></returns>
  [Function( "ConstantGet" )]
  public async Task<IActionResult> ConstantGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "constants/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      Logger.LogInformation( $"ConstantGet" );

      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetAsync( auth, id );
      return request
        .CreateResponse( OLabObjectResult<ConstantsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ConstantGetAsync ) );
    }
  }

  /// <summary>
  /// Saves a object edit
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns>IActionResult</returns>
  [Function( "ConstantPut" )]
  public async Task<IActionResult> ConstantPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "constants/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      Logger.LogInformation( $"ConstantPut" );

      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<ConstantsDto>();

      await _endpoint.PutAsync( auth, id, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ConstantPutAsync ) );
    }

  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "ConstantPost" )]
  public async Task<IActionResult> ConstantPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "constants" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

      Logger.LogInformation( $"ConstantPost" );

      var body = await request.ParseBodyFromRequestAsync<ConstantsDto>();
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<ConstantsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ConstantPostAsync ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "ConstantDelete" )]
  public async Task<IActionResult> ConstantDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "constants/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, id );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ConstantDeleteAsync ) );
    }

  }
}
