using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints.Player;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using OLab.Api.Data;

namespace OLab.FunctionApp.Functions.API.player;

public partial class QuestionResponsesFunction : OLabFunction
{
  private readonly ResponseEndpoint _endpoint;

  public QuestionResponsesFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<QuestionResponsesFunction>(loggerFactory);
    _endpoint = new ResponseEndpoint(Logger, configuration, dbContext);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="body"></param>
  /// <returns></returns>
  [Function("QuestionResponsePostPlayer")]
  public async Task<HttpResponseData> QuestionResponsePostPlayerAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "response/{questionId}")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint questionId
  )
  {

    QuestionResponsePostDataDto body = null;

    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

      var session = new OLabSession(Logger, DbContext, auth.UserContext);
      session.SetMapId(body.MapId);

      var questionPhys = await GetQuestionAsync(body.QuestionId);
      if (questionPhys == null)
        throw new Exception($"Question {body.QuestionId} not found");

      var result =
        await _endpoint.PostQuestionResponseAsync(questionPhys, body);

      session.OnQuestionResponse(
        body,
        questionPhys);

      response = request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects));

    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;
  }
}
