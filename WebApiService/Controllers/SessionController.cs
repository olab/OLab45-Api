using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi
{
  [Route("olab/api/v3/sessions")]
  [ApiController]
  public class SessionController : OLabController
  {
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
    }

    // POST: api/sessions
    /// <summary>
    /// Create new counter
    /// </summary>
    /// <param name="dto">Counter data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult PostSessionRequestAsync([FromBody] SessionRequest dto)
    {
      try
      {
        Guard.Argument(dto).NotNull(nameof(dto));

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        var sessionTraces = DbContext.UserSessionTraces.Where( x =>x.SessionId == 163370 ).ToList();
        FileStreamResult fr = CreateExcelFile.StreamExcelDocument(sessionTraces, "sessionTraces.xlsx");

        return fr;
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
      }
    }

  }
}
