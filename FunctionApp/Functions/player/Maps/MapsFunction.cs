using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
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
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<MapsFunction>(loggerFactory);
      _endpoint = new MapsEndpoint(Logger, _configuration.appSettings, DbContext);
    }

    /// <summary>
    /// Get a list of maps
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [Function("MapsGetPlayer")]
    public async Task<HttpResponseData> MapsGetPlayerAsync(
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

        response = request.CreateResponse(
          OLabObjectPagedListResult<MapsDto>.Result(pagedResult.Data, pagedResult.Remaining));
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
    /// <param name="id"></param>
    /// <returns></returns>
    [Function("MapGetPlayer")]
    public async Task<HttpResponseData> MapGetPlayerAsync(
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
        response = request.CreateResponse(OLabObjectResult<MapsFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Append template to an existing map
    /// </summary>
    /// <param name="mapId">Map to add template to</param>
    /// <param name="CreateMapRequest.templateId">Template to add to map</param>
    /// <returns>IActionResult</returns>
    [Function("MapAppendTemplatePostPlayer")]
    public async Task<HttpResponseData> MapAppendTemplatePostPlayerAsync(
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

        response = request.CreateResponse(OLabObjectResult<ExtendMapResponse>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;

    }

    /// <summary>
    /// Create new map (using optional template)
    /// </summary>
    /// <param name="body">Create map request body</param>
    /// <returns>IActionResult</returns>
    [Function("MapCreatePostPlayer")]
    public async Task<HttpResponseData> MapCreatePostPlayerAsync(
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

        response = request.CreateResponse(OLabObjectResult<MapsFullRelationsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}
