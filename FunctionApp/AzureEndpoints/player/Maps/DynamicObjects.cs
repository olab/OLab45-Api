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
  public partial class MapsAzureEndpoint : OLabAzureEndpoint
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <param name="sinceTime"></param>
    /// <returns></returns>
    [FunctionName("GetDynamicScopedObjectsRaw")]
    public async Task<IActionResult> GetDynamicScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects/raw")] HttpRequest request,
      uint mapId,
      uint nodeId,
      [FromQuery] uint sinceTime = 0      
      )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
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
    [FunctionName("GetDynamicScopedObjectsTranslated")]
    public async Task<IActionResult> GetDynamicScopedObjectsTranslatedAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "maps/{mapId}/nodes/{nodeId}/dynamicobjects")] HttpRequest request,
      uint mapId,
      uint nodeId, 
      [FromQuery] uint sinceTime = 0)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));
        Guard.Argument(mapId, nameof(mapId)).NotZero();
        Guard.Argument(nodeId, nameof(nodeId)).NotZero();

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
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

    /// <summary>
    /// Retrieve dynamic scoped objects for current node
    /// </summary>
    /// <param name="serverId">Server id</param>
    /// <param name="node">Current node</param>
    /// <param name="sinceTime">Look for changes since</param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    public async Task<IActionResult> GetDynamicScopedObjectsAsync(
      uint serverId,
      MapNodes node,
      uint sinceTime,
      bool enableWikiTranslation)
    {
      try
      {
        var dto = await _endpoint.GetDynamicScopedObjectsAsync(serverId, node, sinceTime, enableWikiTranslation); ;
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
