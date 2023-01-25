using Data.Contracts;
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

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    [Route("olab/api/v3/servers")]
    public partial class ReportController : OlabController
    {
        private readonly ReportEndpoint _endpoint;

        public ReportController(ILogger<ReportController> logger, OLabDBContext context) : base(logger, context)
        {
            _endpoint = new ReportEndpoint(this.logger, context);
        }

        /// <summary>
        /// Get a list of servers
        /// </summary>
        /// <param name="take">Max number of records to return</param>
        /// <param name="skip">SKip over a number of records</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAsync([FromQuery] string sessionId)
        {
            try
            {
                var response = await _endpoint.GetAsync(sessionId);
                return OLabObjectResult<SessionReport>.Result(response);
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
