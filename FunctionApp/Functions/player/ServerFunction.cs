using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class ServerFunction : OLabFunction
  {
    private readonly ServerEndpoint _endpoint;

    public ServerFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new ServerEndpoint(Logger, appSettings, DbContext);
    }

    /// <summary>
    /// Get a list of servers
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [Function("ServersGetPlayer")]
    public async Task<HttpResponseData> ServerGetPlayerAsync(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var pagedResponse = await _endpoint.GetAsync(take, skip);
        response = request.CreateResponse( OLabObjectListResult<Servers>.Result(pagedResponse.Data));
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [Function("ServerScopedObjectsRawGetPlayer")]
    public async Task<HttpResponseData> GetScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers/{serverId}/scopedobjects/raw")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint serverId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsRawAsync(serverId);
        response = request.CreateResponse( OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto));
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
    /// <param name="serverId"></param>
    /// <returns></returns>
    [Function("ServerScopedObjectsGetPlayer")]
    public async Task<HttpResponseData> GetScopedObjectsTranslatedAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servers/{serverId}/scopedobjects")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint serverId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
        response = request.CreateResponse( OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}
