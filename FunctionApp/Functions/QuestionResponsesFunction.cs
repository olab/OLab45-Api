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
using OLabWebAPI.Model;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;

namespace OLab.Endpoints.Azure
{
  public class QuestionRepsonsesFunction : OLabFunction
  {
    private readonly QuestionResponsesEndpoint _endpoint;

    public QuestionRepsonsesFunction(
      IUserService userService,
      ILogger<CountersFunction> logger,
      IOptions<AppSettings> appSettings,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));
      Guard.Argument(appSettings).NotNull(nameof(appSettings));

      _endpoint = new QuestionResponsesEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [FunctionName("QuestionResponsePut")]
    public async Task<IActionResult> QuestionResponseGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "response/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);
        var body = await request.ParseBodyFromRequestAsync<QuestionResponsesDto>();

        await _endpoint.PutAsync(auth, id, body);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return new NoContentResult();
    }

    /// <summary>
    /// Create new object
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [FunctionName("QuestionResponsePost")]
    public async Task<IActionResult> QuestionResponsePostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questionresponses")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

        var body = await request.ParseBodyFromRequestAsync<QuestionResponsesDto>();
        var dto = await _endpoint.PostAsync(auth, body);

        return OLabObjectResult<QuestionResponsesDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [FunctionName("QuestionResponseDelete")]
    public async Task<IActionResult> QuestionResponseDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "questionresponses/{id}")] HttpRequest request,
      uint id
    )
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = AuthorizeRequest(request);

      return await _endpoint.DeleteAsync(auth, id);
    }


  }
}
