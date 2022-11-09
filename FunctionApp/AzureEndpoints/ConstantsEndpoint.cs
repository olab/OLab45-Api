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

namespace OLab.FunctionApp.Endpoints
{
  public class ConstantsAzureEndpoint : OLabAzureEndpoint
  {
    private readonly ConstantsEndpoint _endpoint;

    public ConstantsAzureEndpoint(
      IUserService userService,
      ILogger<ConstantsAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      _endpoint = new ConstantsEndpoint(this.logger, context);
    }

    /// <summary>
    /// Gets all constants
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("Get")]
    public async Task<IActionResult> GetAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "constants")] HttpRequest request,
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

        userService.ValidateToken(request);
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
    public async Task<IActionResult> GetByIdAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "constants")] HttpRequest request,
      uint id
    )
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, request);
        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<ConstantsDto>.Result(dto);
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
