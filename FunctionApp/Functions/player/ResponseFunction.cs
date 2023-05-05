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

namespace OLab.Endpoints.Azure.Player
{
  public partial class ResponseFunction : OLabFunction
  {
    private readonly ResponseEndpoint _endpoint;

    public ResponseFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new ResponseEndpoint(this.logger, context);
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
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        body = JsonConvert.DeserializeObject<QuestionResponsePostDataDto>(content);

        var result = await _endpoint.PostQuestionResponseAsync(body);

        userContext.Session.OnQuestionResponse(
          userContext.SessionId,
          body.MapId,
          body.NodeId,
          body.QuestionId,
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
