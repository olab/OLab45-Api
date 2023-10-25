using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;

namespace OLabWebAPI.Endpoints.WebApi;

/// <summary>
/// 
/// </summary>
[Route("olab/api/v3/[controller]/[action]")]
[ApiController]
public class AuthController : OLabController
{
  protected readonly IUserService _userService;
  private readonly IOLabAuthentication _authentication;

  public AuthController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    IOLabAuthentication authentication,
    OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<AuthController>(loggerFactory);
    _authentication = authentication;
    _userService = userService;
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

    var user = _userService.Authenticate(model);
    if (user == null)
      return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result("Username or password is incorrect"));

    var response = _authentication.GenerateJwtToken(user);
    return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));
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
      var response = _authentication.GenerateAnonymousJwtToken(mapId);
      if (response == null)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result("Must be Logged on to Play Map"));

      return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));

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
      var response = _authentication.GenerateExternalJwtToken(model);
      if (response == null)
        return BadRequest(new { statusCode = 401, message = "Invalid external token" });

      return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));

    }
    catch (Exception ex)
    {
      return BadRequest(new { statusCode = 401, message = ex.Message });
    }
  }

}