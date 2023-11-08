using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Dto.Designer;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Designer;

[Route("olab/api/v3/templates")]
[ApiController]
public partial class TemplatesController : OLabController
{
  private readonly TemplateEndpoint _endpoint;

  public TemplatesController(
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

    Logger = OLabLogger.CreateNew<TemplatesController>(loggerFactory);

    _endpoint = new TemplateEndpoint(
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
      var auth = GetAuthorization(HttpContext);

      var pagedResponse = await _endpoint.GetAsync(take, skip);
      return HttpContext.Request.CreateResponse(OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  [HttpGet("links")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public ActionResult Links()
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = _endpoint.Links();
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapNodeLinkTemplateDto>.Result(dto));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  [HttpGet("nodes")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public ActionResult Nodes()
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = _endpoint.Nodes();
      return HttpContext.Request.CreateResponse(OLabObjectResult<MapNodeTemplateDto>.Result(dto));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }
  }
}
