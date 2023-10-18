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
using OLab.Data.Interface;
using OLab.Common.Interfaces;
using Dawn;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/reports")]
public partial class ReportController : OLabController
{
  private readonly ReportEndpoint _endpoint;

  public ReportController(
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
      var auth = GetRequestContext(HttpContext);
      
      SessionReport response = await _endpoint.GetAsync(auth, sessionId);
      return OLabObjectResult<SessionReport>.Result(response);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

  }
}
