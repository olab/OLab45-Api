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
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ScopedObjects;

/// <summary>
/// Azure Function to handle operations related to questions.
/// </summary>
public class QuestionsFunction : OLabFunction
{
  private readonly QuestionsEndpoint _endpoint;

  /// <summary>
  /// Initializes a new instance of the <see cref="QuestionsFunction"/> class.
  /// </summary>
  /// <param name="loggerFactory">The logger factory.</param>
  /// <param name="configuration">The configuration settings.</param>
  /// <param name="dbContext">The database context.</param>
  /// <param name="wikiTagProvider">The wiki tag module provider.</param>
  /// <param name="fileStorageProvider">The file storage module provider.</param>
  public QuestionsFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<QuestionsFunction>( loggerFactory );
    _endpoint = new QuestionsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Retrieves a list of questions with pagination.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="executionContext">The function execution context.</param>
  /// <returns>An IActionResult containing the list of questions or an error response.</returns>
  [Function( "QuestionsGet" )]
  public async Task<IActionResult> QuestionsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "questions" )] HttpRequestData request,
    FunctionContext executionContext)
  {
    try
    {
      Logger.LogInformation( $"FilesGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<QuestionsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionsGetAsync ) );
    }
  }

  /// <summary>
  /// Retrieves a specific question by its ID.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function execution context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="id">The ID of the question.</param>
  /// <returns>An IActionResult containing the question data or an error response.</returns>
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

      Logger.LogInformation( $"QuestionGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetAsync( auth, id );
      return request
        .CreateResponse( OLabObjectResult<QuestionsFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GetAsync ) );
    }
  }

  /// <summary>
  /// Updates an existing question.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function execution context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="id">The ID of the question to update.</param>
  /// <returns>An IActionResult indicating the result of the operation.</returns>
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

      Logger.LogInformation( $"QuestionPut" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>(GetLogger() );

      await _endpoint.PutAsync( auth, id, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionPutAsync ) );
    }
  }

  /// <summary>
  /// Creates a new question.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function execution context.</param>
  /// <returns>An IActionResult containing the created question data or an error response.</returns>
  [Function( "QuestionPost" )]
  public async Task<IActionResult> QuestionPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "questions" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Logger.LogInformation( $"QuestionPostAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>(GetLogger() );

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<QuestionsFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionPostAsync ) );
    }
  }

  /// <summary>
  /// Deletes a question by its ID.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function execution context.</param>
  /// <param name="id">The ID of the question to delete.</param>
  /// <returns>An IActionResult indicating the result of the operation.</returns>
  [Function( "QuestionDelete" )]
  public async Task<IActionResult> QuestionDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "questions/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    uint id)
  {
    try
    {
      Logger.LogInformation( $"QuestionDelete" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, id );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionDeleteAsync ) );
    }
  }
}
