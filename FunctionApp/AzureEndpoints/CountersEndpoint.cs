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

namespace OLab.FunctionApp.Endpoints
{
  public class CountersAzureEndpoint : OLabAzureEndpoint
  {
    private readonly CountersEndpoint _endpoint;

    public CountersAzureEndpoint(
      IUserService userService,
      ILogger<CountersAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      _endpoint = new CountersEndpoint(this.logger, context);
    }

    /// <summary>
    /// Gets all counters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("GetCounter")]
    public async Task<IActionResult> GetAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "counters")] HttpRequest request,
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

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var pagedResult = await _endpoint.GetAsync(take, skip);
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
    [FunctionName("GetCounterById")]
    public async Task<IActionResult> GetByIdAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "counters/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
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
    [FunctionName("PutCounter")]
    public async Task<IActionResult> PutAsync(
      [HttpTrigger(AuthorizationLevel.User, "put", Route = "counters/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        CountersFullDto dto = JsonConvert.DeserializeObject<CountersFullDto>(content);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        await _endpoint.PutAsync(auth, id, dto);
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
    [FunctionName("PostCounter")]
    public async Task<IActionResult> PostAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "counters")] HttpRequest request
    )
    {
      try
      {
        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        CountersFullDto dto = JsonConvert.DeserializeObject<CountersFullDto>(content);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        dto = await _endpoint.PostAsync(auth, dto);

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
    [FunctionName("DeleteCounter")]
    public async Task<IActionResult> DeleteAsync(
      [HttpTrigger(AuthorizationLevel.User, "delete", Route = "counters/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, request);
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
