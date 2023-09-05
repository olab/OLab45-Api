using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Dto.Designer;
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Utils;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Designer
{
  [Route("olab/api/v3/templates")]
  [ApiController]
  public partial class TemplatesController : OlabController
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplatesController(ILogger<TemplatesController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new TemplateEndpoint(this.logger, appSettings, context);
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
        OLabAPIPagedResponse<MapsDto> pagedResponse = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining);
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
    /// <returns></returns>
    [HttpGet("links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult Links()
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        MapNodeLinkTemplateDto dto = _endpoint.Links();
        return OLabObjectResult<MapNodeLinkTemplateDto>.Result(dto);
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
    /// <returns></returns>
    [HttpGet("nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult Nodes()
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        MapNodeTemplateDto dto = _endpoint.Nodes();
        return OLabObjectResult<MapNodeTemplateDto>.Result(dto);
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
