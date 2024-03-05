using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using System;
using System.Linq;

namespace OLabWebAPI.Endpoints.WebApi;

[Route("olab/api/v3/sessions")]
[ApiController]
public class SessionController : OLabController
{
  private readonly SessionEndpoint _endpoint;

  public SessionController(
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

    Logger = OLabLogger.CreateNew<SessionController>(loggerFactory);

    _endpoint = new SessionEndpoint(
      Logger,
      configuration,
      dbContext);
  }

  // POST: api/sessions
  /// <summary>
  /// Retrieve session
  /// </summary>
  /// <param name="sessionId">SessionId</param>
  /// <returns>IActionResult</returns>
  [HttpGet("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public IActionResult GetAsync(string id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = _endpoint.GetAsync(auth, id);

      var sessionTraces = DbContext.UserSessionTraces.Where(x => x.SessionId == 163370).ToList();
      var fr = CreateExcelFile.StreamExcelDocument(sessionTraces, "sessionTraces.xlsx");

      return fr;
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  // POST: api/sessions
  /// <summary>
  /// Retrieve session(s)
  /// </summary>
  /// <param name="request">Request object</param>
  /// <returns>IActionResult</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public IActionResult PostAsync([FromBody] SessionRequest request)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var sessionTraces = DbContext.UserSessionTraces.Where(x => x.SessionId == 163370).ToList();
      var fr = CreateExcelFile.StreamExcelDocument(sessionTraces, "sessionTraces.xlsx");

      return fr;
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

}
