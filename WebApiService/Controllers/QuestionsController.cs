using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Services;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/questions")]
  [ApiController]
  public partial class QuestionsController : OlabController
  {
    private readonly QuestionsEndpoint _endpoint;

    public QuestionsController(ILogger<QuestionsController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new QuestionsEndpoint(this.logger, appSettings, context);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        OLabAPIPagedResponse<QuestionsDto> pagedResult = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<QuestionsDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        QuestionsFullDto dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<QuestionsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Saves a question edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] QuestionsFullDto dto)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
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
    /// Create new question
    /// </summary>
    /// <param name="dto">Question data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] QuestionsFullDto dto)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        dto = await _endpoint.PostAsync(auth, dto);
        return OLabObjectResult<QuestionsFullDto>.Result(dto);
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
