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
using OLab.Data.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ScopedObjects;

public class Counters : OLabFunction
{
  private readonly CountersEndpoint _endpoint;

  public Counters(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<Counters>( loggerFactory );
    _endpoint = new CountersEndpoint( Logger, configuration, DbContext );
  }

  /// <summary>
  /// Gets all counters
  /// </summary>
  /// <param name="request"></param>
  /// <param name="logger"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  [Function( "CountersGet" )]
  public async Task<IActionResult> CountersGetAsync(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "counters" )] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken cancellationToken)
  {
    Guard.Argument( request ).NotNull( nameof( request ) );
    Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );

    try
    {
      Logger.LogInformation( $"CountersGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<CountersDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CountersGetAsync ) );
    }
  }

  /// <summary>
  /// Gets single constant
  /// </summary>
  /// <param name="id">Counter id</param>
  /// <returns></returns>
  [Function( "CounterGet" )]
  public async Task<IActionResult> CounterGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "counters/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      Logger.LogInformation( $"CounterGet" );

      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetAsync( auth, id );
      return request
        .CreateResponse( OLabObjectResult<CountersDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CounterGetAsync ) );
    }
  }

  /// <summary>
  /// Saves a object edit
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns>IActionResult</returns>
  [Function( "CounterPut" )]
  public async Task<IActionResult> CounterPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "counters/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      Logger.LogInformation( $"CounterPut" );

      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();

      await _endpoint.PutAsync( auth, id, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CounterPutAsync ) );
    }

  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "CounterPost" )]
  public async Task<IActionResult> CounterPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "counters" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Logger.LogInformation( $"CounterPostAsync" );
      await request.LogPostContents( GetLogger() );

      var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<CountersFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CounterPostAsync ) );
    }
  }

  /// <summary>
  /// Updates a counter value
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "CounterPropertyPut" )]
  public async Task<IActionResult> CounterValuePut(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "counters/update/{counterId}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint counterId)
  {
    try
    {
      Logger.LogInformation( $"CounterPropertyPut" );

      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<PutCounterValueRequest>();

      var dto = await _endpoint.PutUpdateAsync( auth, counterId, body );
      return request
        .CreateResponse( OLabObjectResult<DynamicScopedObjectsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CounterPostAsync ) );
    }
  }

  /// <summary>
  /// Delete a constant
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "CounterDelete" )]
  public async Task<IActionResult> CounterDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "counters/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Logger.LogInformation( $"CounterDelete" );

      var auth = GetAuthorization( hostContext );
      await _endpoint.DeleteAsync( auth, id );

      response = request.CreateNoContentResponse();
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( CounterDeleteAsync ) );
    }

  }
}
