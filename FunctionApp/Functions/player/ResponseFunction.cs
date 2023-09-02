using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLabWebAPI.Endpoints.Player;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace OLab.Endpoints.Azure.Player
{
  public partial class ResponseFunction : OLabFunction
  {
    private readonly ResponseEndpoint _endpoint;

    public ResponseFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
      IOptions<AppSettings> appSettings,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));
      Guard.Argument(appSettings).NotNull(nameof(appSettings));

      _endpoint = new ResponseEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [FunctionName("QuestionResponsePostPlayer")]
    public async Task<IActionResult> QuestionResponsePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "response/{questionId}")] HttpRequest request,
      uint questionId
    )
    {

      QuestionResponsePostDataDto body = null;

      try
      {
        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

        body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

        SystemQuestions question = await context.SystemQuestions
          .Include(x => x.SystemQuestionResponses)
          .FirstOrDefaultAsync(x => x.Id == body.QuestionId);

        if (question == null)
          throw new Exception($"Question {body.QuestionId} not found");

        DynamicScopedObjectsDto result =
          await _endpoint.PostQuestionResponseAsync(question, body);

        userContext.Session.OnQuestionResponse(
          body.MapId,
          body.NodeId,
          question.Id,
          body.Value);

      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects);

    }
  }
}
