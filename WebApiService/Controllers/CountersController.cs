using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Options;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    [Route("olab/api/v3/counters")]
    [ApiController]
    public partial class CountersController : OlabController
    {
        private readonly CountersEndpoint _endpoint;

        public CountersController(ILogger<CountersController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
        {
            _endpoint = new CountersEndpoint(this.logger, appSettings, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
        {
            try
            {
                OLabAPIPagedResponse<CountersDto> pagedResult = await _endpoint.GetAsync(take, skip);
                return OLabObjectPagedListResult<CountersDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
        public async Task<IActionResult> GetAsync(uint id)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                CountersDto dto = await _endpoint.GetAsync(auth, id);
                return OLabObjectResult<CountersDto>.Result(dto);
            }
            catch (Exception ex)
            {
                if (ex is OLabUnauthorizedException)
                    return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
                return OLabServerErrorResult.Result(ex.Message);
            }
        }

        /// <summary>
        /// Saves a object edit
        /// </summary>
        /// <param name="id">question id</param>
        /// <returns>IActionResult</returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutAsync(uint id, [FromBody] CountersFullDto dto)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                await _endpoint.PutAsync(auth, id, dto);
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
        /// Create new counter
        /// </summary>
        /// <param name="dto">Counter data</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostAsync([FromBody] CountersFullDto dto)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
                dto = await _endpoint.PostAsync(auth, dto);
                return OLabObjectResult<CountersFullDto>.Result(dto);
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
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAsync(uint id)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
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

    }

}
