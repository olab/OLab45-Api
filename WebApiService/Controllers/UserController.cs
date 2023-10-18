using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OLab.Data.Interface;
using OLab.Common.Interfaces;
using Dawn;

namespace OLabWebAPI.Endpoints.WebApi;

/// <summary>
/// 
/// </summary>
[Route("olab/api/v3/[controller]/[action]")]
[ApiController]
public class AuthController : OLabController
{
  public AuthController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext) : base(configuration, userService, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<AuthController>(loggerFactory);
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="model"></param>
  /// <returns></returns>
  [AllowAnonymous]
  [HttpPost]
  public IActionResult Login(LoginRequest model)
  {
    var ipAddress = HttpContext.Request.Headers["x-forwarded-for"].ToString();

    if (string.IsNullOrEmpty(ipAddress))
      ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

    model.Username = model.Username.ToLower();

    Logger.LogDebug($"Login(user = '{model.Username}' ip: {ipAddress})");

    var response = userService.Authenticate(model);
    if (response == null)
      return OLabUnauthorizedObjectResult.Result("Username or password is incorrect");

    return OLabObjectResult<AuthenticateResponse>.Result(response);
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [AllowAnonymous]
  [HttpGet("{mapId}")]
  public IActionResult LoginAnonymous(uint mapId)
  {
    Logger.LogDebug($"LoginAnonymous(mapId = '{mapId}')");

    try
    {
      AuthenticateResponse response = userService.AuthenticateAnonymously(mapId);
      if (response == null)
        return OLabUnauthorizedObjectResult.Result("Must be Logged on to Play Map");

      return OLabObjectResult<AuthenticateResponse>.Result(response);

    }
    catch (Exception ex)
    {
      return BadRequest(new { statusCode = 401, message = ex.Message });
    }
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="model"></param>
  /// <returns>AuthenticateResponse</returns>
  [AllowAnonymous]
  [HttpPost]
  public IActionResult LoginExternal(ExternalLoginRequest model)
  {
    Logger.LogDebug($"LoginExternal(user = '{model.ExternalToken}')");

    try
    {
      AuthenticateResponse response = userService.AuthenticateExternal(model);
      if (response == null)
        return BadRequest(new { statusCode = 401, message = "Invalid external token" });

      return OLabObjectResult<AuthenticateResponse>.Result(response);

    }
    catch (Exception ex)
    {
      return BadRequest(new { statusCode = 401, message = ex.Message });
    }
  }

}