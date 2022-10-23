using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Controllers.Player;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Model;
using OLabWebAPI.Model.ReaderWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Controllers.Designer
{
  [Route("olab/api/v3/templates")]
  [ApiController]
  public partial class TemplatesController : OlabController
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplatesController(ILogger<TemplatesController> logger, OLabDBContext context, HttpRequest request) : base(logger, context, request)
    {
      _endpoint = new TemplateEndpoint(this.logger, context, auth);
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
      return await _endpoint.GetAsync(take, skip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("links")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult Links()
    {
      return _endpoint.Links();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("nodes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult Nodes()
    {
      return _endpoint.Nodes();
    }
  }
}
