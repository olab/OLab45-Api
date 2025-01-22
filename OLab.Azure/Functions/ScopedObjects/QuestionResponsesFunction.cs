using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Dto;
using OLab.Api.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using OLab.Azure.Functions;
using Microsoft.AspNetCore.Mvc;
using OLab.Azure.Extensions;
using Humanizer;

namespace OLab.Azure.Functions.ScopedObjects;

public class QuestionResponsesFunction : OLabFunction
{
  private readonly QuestionResponsesEndpoint _endpoint;

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
  /// Create new object
  /// </summary>
  /// <param name="dto">object data</param>
  /// <returns>IActionResult</returns>
  [Function( "QuestionResponsePost" )]
  public async Task<IActionResult> QuestionResponsePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "questionresponses" )] HttpRequestData request,
    FunctionContext hostContext)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<QuestionResponsesDto>();

      var dto = await _endpoint.PostAsync( auth, body );
      return request
        .CreateResponse( OLabObjectResult<QuestionResponsesDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionResponsePost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "QuestionResponseDelete" )]
  public async Task<IActionResult> QuestionResponseDeleteAsync(
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

      var data = await _endpoint.DeleteAsync( auth, id );

      response = request.CreateResponse();
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "QuestionResponseDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }


}
