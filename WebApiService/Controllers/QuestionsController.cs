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
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using Dawn;
using OLab.Api.ObjectMapper;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/questions")]
[ApiController]
public partial class QuestionsController : OLabController
{
  private readonly QuestionsEndpoint _endpoint;

  public QuestionsController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      userService,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<QuestionsController>(loggerFactory);

    _endpoint = new QuestionsEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
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
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      var pagedResult = await _endpoint.GetAsync(take, skip);
      return OLabObjectPagedListResult<QuestionsDto>.Result(pagedResult.Data, pagedResult.Remaining);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }

  /// <summary>
  /// Gets a specific question
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync(uint id)
  {
    try
    {
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      var dto = await _endpoint.GetAsync(auth, id);
      return OLabObjectResult<QuestionsFullDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
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
      Guard.Argument(id, nameof(id)).NotZero();

      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      await _endpoint.PutAsync(auth, id, dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
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
      Guard.Argument(dto).NotNull(nameof(dto));

      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      dto = await _endpoint.PostAsync(auth, dto);
      return OLabObjectResult<QuestionsFullDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }
}
