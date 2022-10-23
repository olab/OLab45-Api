using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;

using OLabWebAPI.Utils;
using System.Text;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using OLabWebAPI.Common;
using OLabWebAPI.Endpoints.Player;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/response")]
  [ApiController]
  public partial class ResponseController : OlabController
  {
    private readonly ResponseEndpoint _endpoint;

    public ResponseController(ILogger<ResponseController> logger, OLabDBContext context, HttpRequest request) : base(logger, context, request)
    {
      _endpoint = new ResponseEndpoint(this.logger, context, auth);
    }

    /// <summary>
    /// 
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
        var question = await GetQuestionAsync(body.QuestionId);
        if (question == null)
          throw new Exception($"Question {body.QuestionId} not found");

        var result = await _endpoint.PostQuestionResponseAsync(body);

        var userContext = new UserContext(logger, context, HttpContext);
        userContext.Session.OnQuestionResponse(
          userContext.SessionId,
          body.MapId,
          body.NodeId,
          question.Id,
          body.Value);

      }
      catch (Exception ex)
      {
        logger.LogError($"PostQuestionResponseAsync error: {ex.Message}");
        return OLabServerErrorResult.Result(ex.Message);
      }

      return OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects);

    }

    /// <summary>
    /// Get counter for question
    /// </summary>
    /// <param name="question">Source question</param>
    /// <param name="dbCounter">Database Counter</param>
    /// <param name="body">Request body</param>
    /// <returns>Dto counter</returns>
    private CountersDto GetTargetCounter(SystemQuestions question, SystemCounters dbCounter, QuestionResponsePostDataDto body)
    {
      var dynamicCounter = body.DynamicObjects.GetCounter((uint)question.CounterId.Value);
      if (dynamicCounter == null)
        logger.LogError($"Counter {question.CounterId.Value} not found in request. Update ignored");
      else
      {
        // if counter is server-level, then take db value and copy
        // it to dynamic object version, which is passed back to caller
        if (dbCounter.ImageableType == Utils.Constants.ScopeLevelServer)
          dynamicCounter.Value = dbCounter.ValueAsString();
      }

      return dynamicCounter;
    }

    /// <summary>
    /// Process multiple response-based question
    /// </summary>
    /// <param name="question">Source question</param>
    /// <param name="counterDto">Counter Dto</param>
    /// <param name="body">Request body</param>
    private void ProcessMultipleResponseQuestion(SystemQuestions question, CountersDto counterDto, QuestionResponsePostDataDto body)
    {
      // test for no active counter
      if (counterDto == null)
        return;

      if (string.IsNullOrEmpty(body.Value))
        return;

      var score = question.GetScoreFromResponses(body.Value);

      logger.LogDebug($"counter {counterDto.Id} value = {score}");
      counterDto.SetValue(score);
    }

    /// <summary>
    /// Process value-based question
    /// </summary>
    /// <param name="question">Source question</param>
    /// <param name="counterDto">Counter Dto</param>
    /// <param name="body">Request body</param>
    private void ProcessValueQuestion(SystemQuestions question, CountersDto counterDto, QuestionResponsePostDataDto body)
    {
      // test for no active counter
      if (counterDto == null)
        return;
    }

    /// <summary>
    /// Process single response-based question
    /// </summary>
    /// <param name="question">Source question</param>
    /// <param name="counterDto">Counter Dto</param>
    /// <param name="body">Request body</param>
    private void ProcessSingleResponseQuestion(SystemQuestions question, CountersDto counterDto, QuestionResponsePostDataDto body)
    {
      // test for no active counter
      if (counterDto == null)
        return;

      SystemQuestionResponses currentResponse = null;
      SystemQuestionResponses previousResponse = null;
      int currentScore = 0;
      int previousScore = 0;

      int score = counterDto.ValueAsNumber();

      if (body.ResponseId.HasValue)
      {
        currentResponse = question.GetResponse(body.ResponseId.Value);
        if (currentResponse == null)
          throw new Exception($"Question response {body.ResponseId.Value} not found");
        if (currentResponse.Score.HasValue)
          currentScore = currentResponse.Score.Value;
        else
          logger.LogWarning($"Response {body.ResponseId.Value} does not have a score value");
      }
      else
        throw new Exception($"Question id = {question.Id} current response not valid.");

      if (body.PreviousResponseId.HasValue && (body.PreviousResponseId.Value > 0))
      {
        previousResponse = question.GetResponse(body.PreviousResponseId.Value);
        if (previousResponse == null)
          throw new Exception($"Question previous response {body.PreviousResponseId.Value} not found");
        if (previousResponse.Score.HasValue)
          previousScore = previousResponse.Score.Value;
        else
          logger.LogWarning($"Response {body.ResponseId.Value} does not have a score value");
      }

      // back out any previous response value
      if (previousResponse != null)
      {
        logger.LogDebug($"reverting previous question reponse {body.PreviousResponseId.Value} = {previousScore}");
        score -= previousScore;
      }

      // add in current response score
      if (currentResponse != null)
      {
        logger.LogDebug($"adjusting question with reponse {body.ResponseId.Value} = {currentScore}");
        score += currentScore;
      }

      logger.LogDebug($"counter {counterDto.Id} value = {score}");

      counterDto.SetValue(score);

    }

  }
}
