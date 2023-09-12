using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions
{
  public class QuestionsFunction : OLabFunction
  {
    private readonly QuestionsEndpoint _endpoint;

    public QuestionsFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagModules) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<QuestionsFunction>(loggerFactory);
      _endpoint = new QuestionsEndpoint(Logger, appSettings, dbContext, wikiTagModules);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    [Function("QuestionsGet")]
    public async Task<HttpResponseData> QuestionsGetAsync(
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

        response = request.CreateResponse(
          OLabObjectPagedListResult<QuestionsDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("QuestionGet")]
    [HttpGet("{id}")]
    public async Task<HttpResponseData> GetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questions/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);

        response = request.CreateResponse(OLabObjectResult<QuestionsFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [Function("QuestionPut")]
    public async Task<HttpResponseData> QuestionPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "questions/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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

        response = request.CreateResponse(new NoContentResult());
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Create new object
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [Function("QuestionPost")]
    public async Task<HttpResponseData> QuestionPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questions")] HttpRequestData request,
      FunctionContext hostContext)
    {

      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<QuestionsFullDto>();
        var dto = await _endpoint.PostAsync(auth, body);

        response = request.CreateResponse(OLabObjectResult<QuestionsFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}
