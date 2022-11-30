using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OLabWebAPI.Endpoints.Player;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using System.Text;
using OLabWebAPI.Model;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Services;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
    public partial class MapsController : OlabController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/scopedobjects/raw")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/scopedobjects")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetScopedObjectsAsync(uint id)
        {
            try
            {
                var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
                var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
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
