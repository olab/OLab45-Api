using Data.Contracts;
using Dawn;
using Endpoints.player.ReportEndpoint;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/reports")]
public partial class ReportController : OLabController
{
  private readonly ReportEndpoint _endpoint;

  public ReportController(
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

    Logger = OLabLogger.CreateNew<MapsController>(loggerFactory);

    _endpoint = new ReportEndpoint(Logger, configuration, dbContext);
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
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var response = await _endpoint.GetAsync(auth, sessionId);
      return HttpContext.Request.CreateResponse(OLabObjectResult<SessionReport>.Result(response));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }

  }
}
