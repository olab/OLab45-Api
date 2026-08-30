using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ScopedObjects;

/// <summary>
/// Azure Function for handling question responses.
/// </summary>
public class QuestionResponsesFunction : OLabFunction
{
  private readonly QuestionResponsesEndpoint _endpoint;

  /// <summary>
  /// Initializes a new instance of the <see cref="QuestionResponsesFunction"/> class.
  /// </summary>
  /// <param name="loggerFactory">The logger factory.</param>
  /// <param name="configuration">The configuration.</param>
  /// <param name="dbContext">The database context.</param>
  /// <param name="wikiTagProvider">The wiki tag provider.</param>
  public QuestionResponsesFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
    Guard.Argument( wikiTagProvider ).NotNull( nameof( wikiTagProvider ) );

    Logger = OLabLogger.CreateNew<QuestionResponsesFunction>( loggerFactory );

    _endpoint = new QuestionResponsesEndpoint(
      Logger,
      configuration,
      DbContext );
  }

  /// <summary>
  /// Handles the HTTP POST request to create a new question response.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function context.</param>
  /// <returns>The action result.</returns>
  [Function( "QuestionResponsePost" )]
  public async Task<HttpResponseData> QuestionResponsePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "questionresponses" )] HttpRequestData request,
    FunctionContext hostContext)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"QuestionResponsePostAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<QuestionResponsesDto>( GetLogger() );

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<QuestionResponsesDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionResponsePostAsync ) );
    }
  }

  /// <summary>
  /// Handles the HTTP DELETE request to delete a question response by ID.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="id">The ID of the question response to delete.</param>
  /// <returns>The action result.</returns>
  [Function( "QuestionResponseDelete" )]
  public async Task<HttpResponseData> QuestionResponseDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "questionresponses/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id
  )
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      await _endpoint.DeleteAsync( auth, id );

      response = request.CreateResponse();
      return OLabFunctionResponses.OLabNoContentResponse( request );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionResponseDeleteAsync ) );
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
  [Function( "QuestionResponsePut" )]
  public async Task<HttpResponseData> QuestionResponsePutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "questionresponses/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( id, nameof( id ) ).NotZero();
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"QuestionResponsePutAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      var body = await request.ParseBodyFromRequestAsync<QuestionResponsesDto>( GetLogger() );

      await _endpoint.PutAsync( auth, id, body );
      return OLabFunctionResponses.OLabNoContentResponse( request );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( QuestionResponsePutAsync ) );
    }
  }

}
