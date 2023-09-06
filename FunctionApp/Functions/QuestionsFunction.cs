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
using OLab.Api.Data.Exceptions;
using OLab.Api.Utils;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OLab.FunctionApp.Functions
{
  public class QuestionsFunction : OLabFunction
  {
    private readonly QuestionsEndpoint _endpoint;

    public QuestionsFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new QuestionsEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    [Function("QuestionsGet")]
    public async Task<IActionResult> QuestionsGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questions")] HttpRequestData request,
        FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        var pagedResult = await _endpoint.GetAsync(take, skip);
        Logger.LogInformation(string.Format("Found {0} questions", pagedResult.Data.Count));

        return OLabObjectPagedListResult<QuestionsDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
    [Function("QuestionGet")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questions/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<QuestionsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [Function("QuestionPut")]
    public async Task<IActionResult> QuestionPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "questions/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);
        var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>();

        await _endpoint.PutAsync(auth, id, body);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
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
    [Function("QuestionPost")]
    public async Task<IActionResult> QuestionPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questions")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>();
        var dto = await _endpoint.PostAsync(auth, body);

        return OLabObjectResult<QuestionsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

  }
}
