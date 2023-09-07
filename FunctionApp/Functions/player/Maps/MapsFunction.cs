using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
  public partial class MapsFunction : OLabFunction
  {
    private readonly MapsEndpoint _endpoint;

    public MapsFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new MapsEndpoint(Logger, DbContext);
    }

    /// <summary>
    /// Get a list of maps
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [Function("MapsGetPlayer")]
    public async Task<IActionResult> MapsGetPlayerAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps")] HttpRequestData request,
      FunctionContext hostContext
    )
    {
      try
      {
        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        Logger.LogInformation(string.Format("Found {0} maps", pagedResult.Data.Count));

        return OLabObjectPagedListResult<MapsDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapGetPlayer")]
    public async Task<IActionResult> MapGetPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<MapsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Append template to an existing map
    /// </summary>
    /// <param name="mapId">Map to add template to</param>
    /// <param name="CreateMapRequest.templateId">Template to add to map</param>
    /// <returns>IActionResult</returns>
    [Function("MapAppendTemplatePostPlayer")]
    public async Task<IActionResult> MapAppendTemplatePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "maps/{mapId}")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint mapId
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<ExtendMapRequest>();
        var dto = await _endpoint.PostExtendMapAsync(auth, mapId, body);

        return OLabObjectResult<ExtendMapResponse>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Create new map (using optional template)
    /// </summary>
    /// <param name="body">Create map request body</param>
    /// <returns>IActionResult</returns>
    [Function("MapCreatePostPlayer")]
    public async Task<IActionResult> MapCreatePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "maps")] HttpRequestData request,
      FunctionContext hostContext
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

        var body = await request.ParseBodyFromRequestAsync<CreateMapRequest>();
        var dto = await _endpoint.PostCreateMapAsync(auth, body);

        return OLabObjectResult<MapsFullRelationsDto>.Result(dto);
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
