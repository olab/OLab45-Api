using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Data.Exceptions;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Configuration;
using OLabWebAPI.Endpoints;
using Microsoft.Azure.Functions.Worker;

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
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(configuration).NotNull(nameof(configuration));
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(userService).NotNull(nameof(userService));

      _endpoint = new ConstantsEndpoint(logger, appSettings, context);
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "constants")] HttpRequest request,
        CancellationToken cancellationToken)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(logger).NotNull(nameof(logger));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        logger.LogInformation(string.Format("Found {0} constants", pagedResult.Data.Count));

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
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "constants/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

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
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "constants/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

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
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "constants")] HttpRequest request
    )
    {
      try
      {
        var body = await request.ParseBodyFromRequestAsync<ConstantsDto>();

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
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "constants/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

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
