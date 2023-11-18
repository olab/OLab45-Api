using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Data;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.API.player
{
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
        var session = new OLabSession(Logger, DbContext, auth.UserContext);

        body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

        var question = await DbContext.SystemQuestions
          .Include(x => x.SystemQuestionResponses)
          .FirstOrDefaultAsync(x => x.Id == body.QuestionId)
          ?? throw new Exception($"Question {body.QuestionId} not found");

        var result =
          await _endpoint.PostQuestionResponseAsync(question, body);

        session.OnQuestionResponse(
          body.MapId,
          body.NodeId,
          question.Id,
          body.Value);

        response = request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects));

      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }
  }
}
