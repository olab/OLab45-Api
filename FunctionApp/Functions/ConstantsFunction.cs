using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions
{
  public class ConstantsFunction : OLabFunction
  {
    private readonly ConstantsEndpoint _endpoint;

    public ConstantsFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new ConstantsEndpoint(Logger, appSettings, DbContext);
    }

    /// <summary>
    /// Gets all constants
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("ConstantsGet")]
    public async Task<IActionResult> ConstantsGetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "constants")] HttpRequestData request,
        FunctionContext hostContext,
        CancellationToken cancellationToken)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        var auth = GetRequestContext(hostContext);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        Logger.LogInformation(string.Format("Found {0} constants", pagedResult.Data.Count));

        return OLabObjectPagedListResult<ConstantsDto>.Result(pagedResult.Data, pagedResult.Remaining);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);

        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Gets single constant
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [Function("ConstantGet")]
    public async Task<IActionResult> ConstantGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "constants/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));
        Guard.Argument(id, nameof(id)).NotZero();

        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<ConstantsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);

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
    [Function("ConstantPut")]
    public async Task<IActionResult> ConstantPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "constants/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));
        Guard.Argument(id, nameof(id)).NotZero();

        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<ConstantsDto>();

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
    [Function("ConstantPost")]
    public async Task<IActionResult> ConstantPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "constants")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));

        var body = await request.ParseBodyFromRequestAsync<ConstantsDto>();

        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.PostAsync(auth, body);
        return OLabObjectResult<ConstantsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Delete a constant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("ConstantDelete")]
    public async Task<IActionResult> ConstantDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "constants/{id}")] HttpRequestData request,
      FunctionContext hostContext,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(hostContext).NotNull(nameof(hostContext));
        Guard.Argument(id, nameof(id)).NotZero();

        var auth = GetRequestContext(hostContext);

        await _endpoint.DeleteAsync(auth, id);
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
  }
}
