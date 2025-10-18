using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Azure.Functions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

/// <summary>
/// Azure Function for handling question responses from users.
/// </summary>
public partial class UserQuestionResponse : OLabFunction
{
  private readonly ResponseEndpoint _endpoint;

  public UserQuestionResponse(
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

    Logger = OLabLogger.CreateNew<UserQuestionResponse>( loggerFactory );

    _endpoint = new ResponseEndpoint(
      Logger,
      configuration,
      DbContext );
  }

  /// <summary>
  /// A question response was posted
  /// </summary>
  /// <param name="body"></param>
  /// <returns></returns>
  [Function( "UserQuestionResponse" )]
  public async Task<IActionResult> PostQuestionResponseAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "response/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    uint id)
  {

    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"PostQuestionResponseAsync" );
      await request.LogPostContents( GetLogger() );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

      var session = OLabSession.CreateInstance(
        GetLogger(),
        DbContext,
        auth.AuthenticatedContext );

      session.SetMapId( body.MapId );

      var questionPhys = await GetQuestionAsync( body.QuestionId );
      if ( questionPhys == null )
        throw new Exception( $"Question {body.QuestionId} not found" );

      var result =
        await _endpoint.PostQuestionResponseAsync( questionPhys, body );

      session.OnQuestionResponse(
        body,
        questionPhys );

      return request
        .CreateResponse( OLabObjectResult<DynamicScopedObjectsDto>.Result( body.DynamicObjects ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( PostQuestionResponseAsync ) );
    }

  }
}
