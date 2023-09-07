using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Microsoft.Extensions.Logging;

using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLab.Api.Endpoints.Player;
using OLab.Api.Utils;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.FunctionApp.Extensions;
using Microsoft.Extensions.Configuration;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class ResponseFunction : OLabFunction
  {
    private readonly ResponseEndpoint _endpoint;

    public ResponseFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new ResponseEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [Function("QuestionResponsePostPlayer")]
    public async Task<IActionResult> QuestionResponsePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "response/{questionId}")] HttpRequestData request,
      FunctionContext hostContext,
      uint questionId
    )
    {

      QuestionResponsePostDataDto body = null;

      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        body = await request.ParseBodyFromRequestAsync<QuestionResponsePostDataDto>();

        SystemQuestions question = await DbContext.SystemQuestions
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
