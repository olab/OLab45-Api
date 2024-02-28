using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Dto;
using OLab.FunctionApp.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace OLab.FunctionApp.Functions.Player;

public partial class MapsFunction : OLabFunction
{
  /// <summary>
  /// Saves a link edit
  /// </summary>
  /// <param name="mapId">map id</param>
  /// <param name="nodeId">node id</param>
  /// <param name="linkId">link id</param>
  /// <returns>IActionResult</returns>
  [HttpPut("{mapId}/nodes/{nodeId}/links/{linkId}")]
  public async Task<HttpResponseData> PutMapNodeLinksAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "maps/{mapId}/nodes/{nodeId}/links/{linkId}")] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint mapId,
    uint nodeId,
    uint linkId
  )
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);
      var body = await request.ParseBodyFromRequestAsync<MapNodeLinksFullDto>();

      await _endpoint.PutMapNodeLinksAsync(auth, mapId, nodeId, linkId, body);
      response = request.CreateResponse(new NoContentResult());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      response = request.CreateResponse(ex);
    }

    return response;

  }
}
