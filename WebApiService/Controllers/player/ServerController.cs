using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Endpoints.Player;
using OLabWebAPI.Model;
using System;
using System.Threading.Tasks;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    [Route("olab/api/v3/servers")]
    public partial class ServerController : OlabController
    {
        private readonly ServerEndpoint _endpoint;

        public ServerController(ILogger<ServerController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
        {
            _endpoint = new ServerEndpoint(this.logger, appSettings, context);
        }

        /// <summary>
        /// Get a list of servers
        /// </summary>
        /// <param name="take">Max number of records to return</param>
        /// <param name="skip">SKip over a number of records</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
        {
            try
            {
                OLabAPIPagedResponse<Servers> pagedResponse = await _endpoint.GetAsync(take, skip);
                return OLabObjectListResult<Servers>.Result(pagedResponse.Data);
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
        /// <param name="serverId"></param>
        /// <returns></returns>
        [HttpGet("{serverId}/scopedobjects/raw")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetScopedObjectsRawAsync(uint serverId)
        {
            try
            {
                Dto.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsRawAsync(serverId);
                return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
        /// <param name="serverId"></param>
        /// <returns></returns>
        [HttpGet("{serverId}/scopedobjects")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetScopedObjectsTranslatedAsync(uint serverId)
        {
            try
            {
                Dto.ScopedObjectsDto dto = await _endpoint.GetScopedObjectsTranslatedAsync(serverId);
                return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
