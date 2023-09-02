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
using OLabWebAPI.Data.Exceptions;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;

namespace OLab.Endpoints.Azure
{
  public class QuestionsFunction : OLabFunction
  {
    private readonly QuestionsEndpoint _endpoint;

    public QuestionsFunction(
      IUserService userService,
      ILogger<CountersFunction> logger,
      IOptions<AppSettings> appSettings,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new QuestionsEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    [FunctionName("QuestionsGet")]
    public async Task<IActionResult> QuestionsGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questions")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        var pagedResult = await _endpoint.GetAsync(take, skip);
        logger.LogInformation(string.Format("Found {0} questions", pagedResult.Data.Count));

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
    [FunctionName("QuestionGet")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questions/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

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
    [FunctionName("QuestionPut")]
    public async Task<IActionResult> QuestionPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "questions/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);
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
    [FunctionName("QuestionPost")]
    public async Task<IActionResult> QuestionPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questions")] HttpRequest request
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

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
