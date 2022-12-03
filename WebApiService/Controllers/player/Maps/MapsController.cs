using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints.Player;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    [Route("olab/api/v3/maps")]
    [ApiController]
    public partial class MapsController : OlabController
    {
        private readonly MapsEndpoint _endpoint;

        public MapsController(ILogger<MapsController> logger, OLabDBContext context) : base(logger, context)
        {
            _endpoint = new MapsEndpoint(this.logger, context);
        }

        /// <summary>
        /// Get a list of maps
        /// </summary>
        /// <param name="take">Max number of records to return</param>
        /// <param name="skip">SKip over a number of records</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMapsPlayerAsync([FromQuery] int? take, [FromQuery] int? skip)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                OLabAPIPagedResponse<MapsDto> pagedResponse = await _endpoint.GetAsync(auth, take, skip);
                return OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining);
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
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMapPlayerAsync(uint id)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                MapsFullDto dto = await _endpoint.GetAsync(auth, id);
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
        [HttpPost("{mapId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostAppendTemplateToMapPlayerAsync([FromRoute] uint mapId, [FromBody] ExtendMapRequest body)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                ExtendMapResponse dto = await _endpoint.PostExtendMapAsync(auth, mapId, body);
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
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostCreateMapAsync([FromBody] CreateMapRequest body)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                MapsFullRelationsDto dto = await _endpoint.PostCreateMapAsync(auth, body);
                return OLabObjectResult<MapsFullRelationsDto>.Result(dto);
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
        /// <param name="mapdto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutAsync(uint id, MapsFullDto mapdto)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                await _endpoint.PutAsync(auth, id, mapdto);
            }
            catch (Exception ex)
            {
                if (ex is OLabUnauthorizedException)
                    return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
                return OLabServerErrorResult.Result(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAsync(uint id)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                await _endpoint.DeleteAsync(auth, id);
            }
            catch (Exception ex)
            {
                if (ex is OLabUnauthorizedException)
                    return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
                return OLabServerErrorResult.Result(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        [HttpGet("{mapId}/links")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLinksAsync(uint mapId)
        {
            try
            {
                OLabWebApiAuthorization auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                System.Collections.Generic.IList<MapNodeLinksFullDto> dtoList = await _endpoint.GetLinksAsync(auth, mapId);
                return OLabObjectListResult<MapNodeLinksFullDto>.Result(dtoList);
            }
            catch (Exception ex)
            {
                if (ex is OLabUnauthorizedException)
                    return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
                return OLabServerErrorResult.Result(ex.Message);
            }

        }

        [HttpOptions]
        public void Options()
        {

        }
    }
}
