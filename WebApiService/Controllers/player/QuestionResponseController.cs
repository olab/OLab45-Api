using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Services;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/response")]
  [ApiController]
  public partial class QuestionResponseController : OlabController
  {
    private readonly ResponseEndpoint _endpoint;

    public QuestionResponseController(ILogger<QuestionResponseController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new ResponseEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// A question response was posted
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost("{questionId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostQuestionResponseAsync(
      [FromBody] QuestionResponsePostDataDto body)
    {

      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);

        Data.Interface.IUserContext userContext = auth.GetUserContext();
        _endpoint.SetUserContext(userContext);

        SystemQuestions question = await GetQuestionAsync(body.QuestionId);
        if (question == null)
          throw new Exception($"Question {body.QuestionId} not found");

        DynamicScopedObjectsDto result =
          await _endpoint.PostQuestionResponseAsync(question, body);

        userContext.Session.OnQuestionResponse(
          body.MapId,
          body.NodeId,
          question.Id,
          body.Value);

      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects);

    }
  }
}
