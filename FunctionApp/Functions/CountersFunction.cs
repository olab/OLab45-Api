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
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;
using OLabWebAPI.Data.Exceptions;

namespace OLab.Endpoints.Azure
{
  public class CountersFunction : OLabFunction
  {
    private readonly CountersEndpoint _endpoint;

    public CountersFunction(
      IUserService userService,
      ILogger<CountersFunction> logger, 
      IOptions<AppSettings> appSettings,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(appSettings).NotNull(nameof(appSettings));

      _endpoint = new CountersEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// Gets all counters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("CountersGet")]
    public async Task<IActionResult> CountersGetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "counters")] HttpRequest request,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(logger).NotNull(nameof(logger));

      try
      {
        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        logger.LogInformation(string.Format("Found {0} counters", pagedResult.Data.Count));

        return OLabObjectPagedListResult<CountersDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
    /// <param name="id">Counter id</param>
    /// <returns></returns>
    [FunctionName("CounterGet")]
    public async Task<IActionResult> CounterGetAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "counters/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<CountersDto>.Result(dto);
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
    [FunctionName("CounterPut")]
    public async Task<IActionResult> CounterPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "counters/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);
        var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();

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
    [FunctionName("CounterPost")]
    public async Task<IActionResult> CounterPostAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "counters")] HttpRequest request
    )
    {
      try
      {
        var body = await request.ParseBodyFromRequestAsync<CountersFullDto>();

        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);

        var dto = await _endpoint.PostAsync(auth, body);

        return OLabObjectResult<CountersFullDto>.Result(dto);
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
    [FunctionName("CounterDelete")]
    public async Task<IActionResult> CounterDeleteAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "counters/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        var auth = AuthorizeRequest(request);
        
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
