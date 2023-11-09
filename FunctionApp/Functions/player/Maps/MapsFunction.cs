using Dawn;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
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
      IOLabConfiguration configuration,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
        configuration,
        dbContext,
        wikiTagProvider,
        fileStorageProvider)

    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

      Logger = OLabLogger.CreateNew<MapsFunction>(loggerFactory);

      _endpoint = new MapsEndpoint(
        Logger,
        configuration,
        DbContext,
        wikiTagProvider, 
        fileStorageProvider);

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
        var auth = GetAuthorization(hostContext);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        Logger.LogInformation(string.Format("Found {0} maps", pagedResult.Data.Count));

        response = request.CreateResponse(
          OLabObjectPagedListResult<MapsDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
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
        var auth = GetAuthorization(hostContext);

        var dto = await _endpoint.GetAsync(auth, id);
        response = request.CreateResponse(OLabObjectResult<MapsFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
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
        var auth = GetAuthorization(hostContext);

        var body = await request.ParseBodyFromRequestAsync<ExtendMapRequest>();
        var dto = await _endpoint.PostExtendMapAsync(auth, mapId, body);

        response = request.CreateResponse(OLabObjectResult<ExtendMapResponse>.Result(dto));
      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
        response = request.CreateResponse(ex);
      }

      return response;

    }

    /// <summary>
    /// Append template to an existing map
    /// </summary  
    /// <param name="mapId">Map to add template to</param>
    /// <param name="CreateMapRequest.templateId">Template to add to map</param>
    /// <returns>IActionResult</returns>
    [Function("MapPutPlayer")]
    public async Task<HttpResponseData> MapPutPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}")] HttpRequestData request,
      FunctionContext hostContext, 
      CancellationToken cancellationToken,
      uint mapId
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var body = await request.ParseBodyFromRequestAsync<MapsFullDto>();
        await _endpoint.PutAsync(auth, mapId, body);

        return request.CreateResponse(OLabObjectResult<MapsFullDto>.Result(body));

      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
        return request.CreateResponse(ex);
      }

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
        var auth = GetAuthorization(hostContext);

        var body = await request.ParseBodyFromRequestAsync<CreateMapRequest>();
        var dto = await _endpoint.PostCreateMapAsync(auth, body);

        response = request.CreateResponse(OLabObjectResult<MapsFullRelationsDto>.Result(dto));
      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Gets the links for a map
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns>MapNodeLinks dto</returns>
    [Function("MapLinksGetPlayer")]
    public async Task<HttpResponseData> MapLinksGetPlayerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/links")] HttpRequestData request,
      FunctionContext hostContext, CancellationToken cancellationToken,
      uint id
    )
    {
      try
      {
        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var dtoList = await _endpoint.GetLinksAsync(auth, id);
        Logger.LogInformation(string.Format("Found {0} map links", dtoList.Count));

        response = request.CreateResponse(OLabObjectListResult<MapNodeLinksFullDto>.Result(dtoList));
      }
      catch (Exception ex)
      {
        Logger.LogError($"{ex.Message} {ex.StackTrace}");
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}
