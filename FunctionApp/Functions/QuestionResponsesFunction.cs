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
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions
{
  public class QuestionRepsonsesFunction : OLabFunction
  {
    private readonly QuestionResponsesEndpoint _endpoint;

    public QuestionRepsonsesFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<QuestionRepsonsesFunction>(loggerFactory);
      _endpoint = new QuestionResponsesEndpoint(Logger, appSettings, dbContext);
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [Function("QuestionResponsePut")]
    public async Task<HttpResponseData> QuestionResponseGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "response/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
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
    [Function("QuestionResponsePost")]
    public async Task<HttpResponseData> QuestionResponsePostAsync(
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

        response = request.CreateResponse(OLabObjectResult<QuestionResponsesDto>.Result(dto));
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
    [Function("QuestionResponseDelete")]
    public async Task<HttpResponseData> QuestionResponseDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "questionresponses/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var data = await _endpoint.DeleteAsync(auth, id);

        response = request.CreateResponse();
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }


  }
}
