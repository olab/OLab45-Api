using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Services;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  public partial class MapsController : OlabController
  {
    /// <summary>
    /// Saves a link edit
    /// </summary>
    /// <param name="mapId">map id</param>
    /// <param name="nodeId">node id</param>
    /// <param name="linkId">link id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{mapId}/nodes/{nodeId}/links/{linkId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutMapNodeLinksAsync(uint mapId, uint nodeId, uint linkId, [FromBody] MapNodeLinksFullDto linkdto)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        await _endpoint.PutMapNodeLinksAsync(auth, mapId, nodeId, linkId, linkdto);
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
