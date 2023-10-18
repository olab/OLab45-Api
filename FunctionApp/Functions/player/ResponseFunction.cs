using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class ResponseFunction : OLabFunction
  {
    private readonly ResponseEndpoint _endpoint;

    public ResponseFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<ResponseFunction>(loggerFactory);
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
        var auth = GetRequestContext(hostContext);

        body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

        var question = await DbContext.SystemQuestions
          .Include(x => x.SystemQuestionResponses)
          .FirstOrDefaultAsync(x => x.Id == body.QuestionId)
          ?? throw new Exception($"Question {body.QuestionId} not found");

        var result =
          await _endpoint.PostQuestionResponseAsync(question, body);

        auth.GetUserContext().Session.OnQuestionResponse(
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
