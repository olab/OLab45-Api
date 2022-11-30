using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using OLabWebAPI.Model.ReaderWriter;
using System;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Services;

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
                var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
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
