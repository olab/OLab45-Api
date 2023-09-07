using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.Player
{
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
    public async Task<IActionResult> PutMapNodeLinksAsync(
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
        var auth = GetRequestContext(hostContext);
        var body = await request.ParseBodyFromRequestAsync<MapNodeLinksFullDto>();

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
