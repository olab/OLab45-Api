using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;
using OLabWebAPI.Services;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/questionresponses")]
  [ApiController]
  public partial class QuestionResponsesController : OlabController
  {
    private readonly QuestionResponsesEndpoint _endpoint;

    public QuestionResponsesController(ILogger<QuestionsController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new QuestionResponsesEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] QuestionResponsesDto dto)
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
    /// Create new object
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] QuestionResponsesDto dto)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
        dto = await _endpoint.PostAsync(auth, dto);
        return OLabObjectResult<QuestionResponsesDto>.Result(dto);
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
      var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
      return await _endpoint.DeleteAsync(auth, id);
    }

  }

}
