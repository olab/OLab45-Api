using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using System;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Common;
using OLabWebAPI.Services;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/constants")]
  [ApiController]
  public partial class ConstantsController : OlabController
  {
    private readonly ConstantsEndpoint _endpoint;

    public ConstantsController(ILogger<ConstantsController> logger, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new ConstantsEndpoint(this.logger, context);
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
        var pagedResult = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<ConstantsDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<ConstantsDto>.Result(dto);
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
    public async Task<IActionResult> PutAsync(uint id, [FromBody] ConstantsDto dto)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
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
    /// Create new object
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] ConstantsDto dto)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        dto = await _endpoint.PostAsync(auth, dto);
        return OLabObjectResult<ConstantsDto>.Result(dto);
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
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
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
