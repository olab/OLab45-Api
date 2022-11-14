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
using OLabWebAPI.Utils;

namespace OLab.Endpoints.Azure.Player
{
  public partial class MapsFunction : OLabFunction
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [FunctionName("MapNodeDynamicScopedObjectsRaw")]
    public async Task<IActionResult> MapNodeDynamicScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects/raw")] HttpRequest request,
      uint mapId,
      uint nodeId
      )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        uint sinceTime = 0;
        if ( request.Query.ContainsKey("sinceTime") )
          sinceTime = (uint)Convert.ToInt32(request.Query["sinceTime"]);

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetDynamicScopedObjectsRawAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [FunctionName("MapNodeDynamicScopedObjects")]
    public async Task<IActionResult> MapNodeDynamicScopedObjectsAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects")] HttpRequest request,
      uint mapId,
      uint nodeId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        uint sinceTime = 0;
        if ( request.Query.ContainsKey("sinceTime") )
          sinceTime = (uint)Convert.ToInt32(request.Query["sinceTime"]);

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetDynamicScopedObjectsTranslatedAsync(auth, mapId, nodeId, sinceTime);
        return OLabObjectResult<DynamicScopedObjectsDto>.Result(dto);
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
