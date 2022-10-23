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

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/response")]
  [ApiController]
  public partial class ResponseController : OlabController
  {
    public ResponseController(ILogger<NodesController> logger, OLabDBContext context) : base(logger, context)
    {
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
      logger.LogDebug($"PostQuestionResponseAsync(questionId={body.QuestionId}, response={body.PreviousValue}->{body.Value}");

      try
      {
        var userContext = new UserContext(logger, context, HttpContext);

        var question = await GetQuestionAsync(body.QuestionId);
        if (question == null)
          throw new Exception($"Question {body.QuestionId} not found");

        // test if counter associated with the question
        if (question.CounterId.HasValue && (question.CounterId.Value > 0))
        {
          var dbCounter = await GetCounterAsync((uint)question.CounterId.Value);
          if (dbCounter == null)
            throw new Exception($"Counter {question.CounterId.Value} not found in database");

          var counterDto = GetTargetCounter(question, dbCounter, body);

          if (question.SystemQuestionResponses.Count > 0)
          {
            if (question.EntryTypeId == 4)
              // handle questions that have a single response
              ProcessSingleResponseQuestion(question, counterDto, body);
            else if (question.EntryTypeId == 3)
              // handle questions that have multiple responses
              ProcessMultipleResponseQuestion(question, counterDto, body);
            else
              throw new NotImplementedException($"question {question.Id} not implemented");
          }
          else
            // handle questions that have no underlying responses (e.g. slider)
            ProcessValueQuestion(question, counterDto, body);

          // if a server-level counter value has changed, write it to db
          if (dbCounter.ImageableType == Utils.Constants.ScopeLevelServer)
          {
            dbCounter.ValueFromNumber(counterDto.ValueAsNumber());
            context.SystemCounters.Update(dbCounter);
            await context.SaveChangesAsync();
          }

          userContext.Session.OnQuestionResponse(
            userContext.SessionId,
            body.MapId,
            body.NodeId,
            question.Id,
            body.Value);
        }
        else
          logger.LogWarning($"question {question.Id} response: question has no counter");

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
