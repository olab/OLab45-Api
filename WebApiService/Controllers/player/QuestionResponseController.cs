using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints.Player;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/response")]
[ApiController]
public partial class QuestionResponseController : OLabController
{
  private readonly ResponseEndpoint _endpoint;

  public QuestionResponseController(
  ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  OLabDBContext dbContext,
  IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
  IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
    configuration,
    dbContext,
    wikiTagProvider,
    fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<QuestionResponseController>(loggerFactory);

    _endpoint = new ResponseEndpoint(
      Logger,
      configuration,
      DbContext);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var question = await GetQuestionAsync(body.QuestionId);
      if (question == null)
        throw new Exception($"Question {body.QuestionId} not found");

      var result =
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
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }

    return HttpContext.Request.CreateResponse(OLabObjectResult<DynamicScopedObjectsDto>.Result(body.DynamicObjects));

  }
}
