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
  public partial class MapsFunction : OLabFunction
  {
    private readonly MapsEndpoint _endpoint;

    public MapsFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new MapsEndpoint(this.logger, context);
    }

    /// <summary>
    /// Get a list of maps
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [FunctionName("MapsGetPlayer")]
    public async Task<IActionResult> MapsGetPlayerAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps")] HttpRequest request
    )
    {
      try
      {
        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        logger.LogInformation(string.Format("Found {0} maps", pagedResult.Data.Count));

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
    [FunctionName("MapGetPlayer")]
    public async Task<IActionResult> MapGetPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{id}")] HttpRequest request,
      uint id
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

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
    [FunctionName("MapAppendTemplatePostPlayer")]
    public async Task<IActionResult> MapAppendTemplatePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "maps/{mapId}")] HttpRequest request,
      uint mapId
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        ExtendMapRequest body = JsonConvert.DeserializeObject<ExtendMapRequest>(content);

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
    [FunctionName("MapCreatePostPlayer")]
    public async Task<IActionResult> MapCreatePostPlayerAsync(
      [HttpTrigger(AuthorizationLevel.User, "post", Route = "maps")] HttpRequest request
    )
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        CreateMapRequest body = JsonConvert.DeserializeObject<CreateMapRequest>(content);

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
