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
  public partial class MapsAzureEndpoint : OLabFunction
  {
    /// <summary>
    /// Saves a link edit
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <param name="linkId">link id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{mapId}/nodes/{nodeId}/links/{linkId}")]
    public async Task<IActionResult> PutMapNodeLinksAsync(
      [HttpTrigger(AuthorizationLevel.User, "put", Route = "maps/{mapId}/nodes/{nodeId}/links/{linkId}")] HttpRequest request,
      uint mapId,
      uint nodeId,
      uint linkId
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var content = await new StreamReader(request.Body).ReadToEndAsync();
        MapNodeLinksFullDto body = JsonConvert.DeserializeObject<MapNodeLinksFullDto>(content);

        await _endpoint.PutMapNodeLinksAsync(auth, mapId, nodeId, linkId, body);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return new NoContentResult();

    }
  }
}
