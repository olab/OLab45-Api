using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Microsoft.Extensions.Logging;

using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLab.Api.Utils;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OLab.FunctionApp.Functions
{
  public class QuestionRepsonsesFunction : OLabFunction
  {
    private readonly QuestionResponsesEndpoint _endpoint;

    public QuestionRepsonsesFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new QuestionResponsesEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [Function("QuestionResponsePut")]
    public async Task<IActionResult> QuestionResponseGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "response/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id
    )
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);
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
    [Function("QuestionResponsePost")]
    public async Task<IActionResult> QuestionResponsePostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questionresponses")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
    [Function("QuestionResponseDelete")]
    public async Task<IActionResult> QuestionResponseDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "questionresponses/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id
    )
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = GetRequestContext(hostContext);

      return await _endpoint.DeleteAsync(auth, id);
    }


  }
}
