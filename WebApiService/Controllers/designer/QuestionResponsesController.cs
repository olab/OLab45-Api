using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Endpoints.WebApi;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Controllers.designer;

[Route("olab/api/v3/questionresponses")]
[ApiController]
public partial class QuestionResponsesController : OLabController
{
  private readonly QuestionResponsesEndpoint _endpoint;

  public QuestionResponsesController(
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
    Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));

    Logger = OLabLogger.CreateNew<QuestionResponsesController>(loggerFactory);
    _endpoint = new QuestionResponsesEndpoint(Logger, configuration, dbContext);
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
      Guard.Argument(id, nameof(id)).NotZero();
      Guard.Argument(dto).NotNull(nameof(dto));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      await _endpoint.PutAsync(auth, id, dto);
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      dto = await _endpoint.PostAsync(auth, dto);
      return HttpContext.Request.CreateResponse(OLabObjectResult<QuestionResponsesDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
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
    Guard.Argument(id, nameof(id)).NotZero();

    // validate token/setup up common properties
    var auth = GetAuthorization(HttpContext);

    return await _endpoint.DeleteAsync(auth, id);
  }

}
