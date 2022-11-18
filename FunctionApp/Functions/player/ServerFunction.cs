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
using OLabWebAPI.Endpoints.Player;

namespace OLab.Endpoints.Azure.Player
{
  public partial class ServerFunction : OLabFunction
  {
    private readonly ServerEndpoint _endpoint;

    public ServerFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new ServerEndpoint(this.logger, context);
    }

    /// <summary>
    /// Get a list of servers
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [FunctionName("ServersGetPlayer")]
    public async Task<IActionResult> ServerGetPlayerAsync(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var pagedResponse = await _endpoint.GetAsync(take, skip);
        return OLabObjectListResult<Servers>.Result(pagedResponse.Data);
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [FunctionName("ServerScopedObjectsRawGetPlayer")]
    public async Task<IActionResult> GetScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers/{serverId}/scopedobjects/raw")] HttpRequest request,
      uint serverId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsRawAsync(serverId);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [FunctionName("ServerScopedObjectsGetPlayer")]
    public async Task<IActionResult> GetScopedObjectsTranslatedAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers/{serverId}/scopedobjects")] HttpRequest request,
      uint serverId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
