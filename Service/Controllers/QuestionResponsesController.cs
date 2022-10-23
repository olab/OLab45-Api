using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI;
using OLabWebAPI.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

using OLabWebAPI.Services;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/questionresponses")]
  [ApiController]
  public partial class QuestionResponsesController : OlabController
  {
    private readonly QuestionResponsesEndpoint _endpoint;

    public QuestionResponsesController(ILogger<QuestionsController> logger, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new QuestionResponsesEndpoint(this.logger, context, auth);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool Exists(uint id)
    {
      return context.SystemQuestionResponses.Any(e => e.Id == id);
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
      var result = await _endpoint.PutAsync(id, dto);
      if ( result != null )
        return result;
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
      return await _endpoint.PostAsync(dto);
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
      return await _endpoint.DeleteAsync(id);
    }

  }

}
