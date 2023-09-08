using Data.Contracts;
using Endpoints.player.ReportEndpoint;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.Threading.Tasks;
using OLabWebAPI.Services;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/reports")]
  public partial class ReportController : OlabController
  {
    private readonly ReportEndpoint _endpoint;

    public ReportController(ILogger<ReportController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, appSettings, context)
    {
      _endpoint = new ReportEndpoint(this.logger, appSettings, context);
    }

    /// <summary>
    /// Get a list of servers
    /// </summary>
    /// <param name="take">Max number of records to return</param>
    /// <param name="skip">SKip over a number of records</param>
    /// <returns>IActionResult</returns>
    [HttpGet("{sessionId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetReportAsync(string sessionId)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);
        SessionReport response = await _endpoint.GetAsync(auth, sessionId);
        return OLabObjectResult<SessionReport>.Result(response);
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
