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

namespace OLab.Endpoints.Azure
{
  public class ConstantsAzureEndpoint : OLabFunction
  {
    private readonly ConstantsEndpoint _endpoint;

    public ConstantsAzureEndpoint(
      IUserService userService,
      ILogger<ConstantsAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new ConstantsEndpoint(this.logger, context);
    }

    /// <summary>
    /// Gets all constants
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("ConstantsGet")]
    public async Task<IActionResult> ConstantsGetAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "constants")] HttpRequest request,
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

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var pagedResult = await _endpoint.GetAsync(take, skip);
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
    [FunctionName("ConstantGet")]
    public async Task<IActionResult> ConstantGetAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "constants/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        AuthorizeRequest(request);

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
    [FunctionName("ConstantPut")]
    public async Task<IActionResult> ConstantPutAsync(
      [HttpTrigger(AuthorizationLevel.User, "put", Route = "constants/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        ConstantsDto dto = JsonConvert.DeserializeObject<ConstantsDto>(content);
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
    [FunctionName("ConstantPost")]
    public async Task<IActionResult> ConstantPostAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "constants")] HttpRequest request
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        ConstantsDto dto = JsonConvert.DeserializeObject<ConstantsDto>(content);

        dto = await _endpoint.PostAsync(auth, dto);
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
    [FunctionName("ConstantDelete")]
    public async Task<IActionResult> ConstantDeleteAsync(
      [HttpTrigger(AuthorizationLevel.User, "delete", Route = "constants/{id}")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        AuthorizeRequest(request);
        
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
