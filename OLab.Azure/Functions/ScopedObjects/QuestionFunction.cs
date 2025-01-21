using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Dto;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace OLab.Azure.Functions.ScopedObjects;

public class QuestionFunction : OLabFunction
{
  private readonly QuestionsEndpoint _endpoint;

  public QuestionFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<QuestionFunction>( loggerFactory );
    _endpoint = new QuestionsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="take"></param>
  /// <param name="skip"></param>
  /// <returns></returns>
  [Function( "QuestionsGet" )]
  public async Task<IActionResult> QuestionsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "questions" )] HttpRequestData request,
    FunctionContext hostContext)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      var queryTake = Convert.ToInt32( request.Query[ "take" ] );
      var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
      int? take = queryTake > 0 ? queryTake : null;
      int? skip = querySkip > 0 ? querySkip : null;

      var result = await _endpoint.GetAsync( take, skip );
      Logger.LogInformation( string.Format( "Found {0} questions", result.Data.Count ) );

      return request
        .CreateResponse( OLabObjectPagedListResult<QuestionsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionsGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "QuestionGet" )]
  [HttpGet( "{id}" )]
  public async Task<IActionResult> GetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "questions/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetAsync( auth, id );
      return request
        .CreateResponse( OLabObjectResult<QuestionsFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionsGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Saves a object edit
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns>IActionResult</returns>
  [Function( "QuestionPut" )]
  public async Task<IActionResult> QuestionPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "questions/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken,
    uint id)
  {

    try
    {
      Guard.Argument( id, nameof( id ) ).NotZero();
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>();

      await _endpoint.PutAsync( auth, id, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionPut" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "QuestionPost" )]
  public async Task<IActionResult> QuestionPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "questions" )] HttpRequestData request,
    FunctionContext hostContext)
  {

    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>();

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<QuestionsFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionPost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "QuestionDelete" )]
  public async Task<IActionResult> QuestionDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "questions/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    uint id)
  {

    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, id );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }
}
