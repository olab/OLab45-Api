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
  public class CountersFunction : OLabFunction
  {
    private readonly CountersEndpoint _endpoint;

    public CountersFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = new OLabLogger(loggerFactory, loggerFactory.CreateLogger<CountersFunction>());
      _endpoint = new CountersEndpoint(Logger, appSettings, DbContext);
    }

    /// <summary>
    /// Gets all counters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("CountersGet")]
    public async Task<HttpResponseData> CountersGetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "counters")] HttpRequestData request,
        FunctionContext hostContext,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(hostContext).NotNull(nameof(hostContext));

      try
      {
        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        var auth = GetRequestContext(hostContext);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        Logger.LogInformation(string.Format("Found {0} counters", pagedResult.Data.Count));

        response = request.CreateResponse(OLabObjectPagedListResult<CountersDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Gets single constant
    /// </summary>
    /// <param name="id">Counter id</param>
    /// <returns></returns>
    [Function("CounterGet")]
    public async Task<HttpResponseData> CounterGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "counters/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));
        Guard.Argument(id, nameof(id)).NotZero();

        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        response = request.CreateResponse(OLabObjectResult<CountersDto>.Result(dto));
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
    [Function("CounterPut")]
    public async Task<HttpResponseData> CounterPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "counters/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));
        Guard.Argument(id, nameof(id)).NotZero();

        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();

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
    [Function("CounterPost")]
    public async Task<HttpResponseData> CounterPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "counters")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();

        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.PostAsync(auth, body);

        response = request.CreateResponse(OLabObjectResult<CountersFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Delete a constant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("CounterDelete")]
    public async Task<HttpResponseData> CounterDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "counters/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id)
    {
      try
      {
        var auth = GetRequestContext(hostContext);

        await _endpoint.DeleteAsync(auth, id);

        response = request.CreateResponse(new NoContentResult());
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }
  }
}
