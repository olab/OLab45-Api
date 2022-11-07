using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OLabWebAPI.Services;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using OLabWebAPI.Common;

namespace OLabWebAPI.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [Route("olab/api/v3/[controller]/[action]")]
  [ApiController]
  public class AuthController : OlabController
  {
    private readonly IUserService _userService;
    protected readonly OLabDBContext _context;
    private readonly AppSettings _appSettings;

    public AuthController(IOptions<AppSettings> appSettings, IUserService userService, ILogger<AuthController> logger, OLabDBContext context)
      : base(logger, context)
    {
      _userService = userService;
      _context = context;
      _appSettings = appSettings.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private RefreshToken CreateRefreshToken()
    {
      var randomNumber = new byte[32];
      using (var generator = RandomNumberGenerator.Create())
      {
        generator.GetBytes(randomNumber);
        return new RefreshToken
        {
          Token = Convert.ToBase64String(randomNumber),
          Expires = DateTime.UtcNow.AddDays(10),
          Created = DateTime.UtcNow
        };

      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ChangePassword(ChangePasswordRequest model)
    {
      logger.LogDebug($"ChangePassword(user = '{model.Username}')");

      // authenticate target user with their password
      var response = _userService.Authenticate(
        new LoginRequest
        {
          Username = model.Username,
          Password = model.Password
        });

      if (response == null)
        return BadRequest(new { message = "Username or password is incorrect" });

      var user = _userService.GetByUserName(model.Username);
      _userService.ChangePassword(user, model);

      context.Users.Update(user);

      return Ok();
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
      logger.LogDebug($"Login(user = '{model.Username}')");

      var response = _userService.Authenticate(model);
      if (response == null)
        return BadRequest(new { statusCode = 401, message = "Username or password is incorrect" });

      return Ok(response);
    }

    /// <summary>
    /// Interactive login
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public IActionResult LoginExternal(ExternalLoginRequest model)
    {
      logger.LogDebug($"LoginExternal(user = '{model.ExternalToken}')");

      var response = _userService.AuthenticateExternal(model);
      if (response == null)
        return BadRequest(new { statusCode = 401, message = "Invalid external token" });

      return Ok(response);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult AddUsers()
    {
      logger.LogDebug($"AddUsers()");

      // test if user has access to add users.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("X", "UserAdmin", 0))
        return OLabUnauthorizedResult.Result();

      var httpRequest = HttpContext.Request;
      var postedFile = httpRequest.Form["File"];

      return OLabObjectResult<AddUserResponse>.Result(new AddUserResponse());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddUser(AddUserRequest model)
    {
      logger.LogDebug($"AddUser(user = '{model.Username}')");

      // test if user has access to add users.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("X", "UserAdmin", 0))
        return OLabUnauthorizedResult.Result();

      var user = _userService.GetByUserName(model.Username);
      if (user != null)
        throw new Exception($"User {model.Username} already exists");

      var newUser = Users.CreateDefault(model);
      var newPassword = newUser.Password;

      _userService.ChangePassword(newUser, new ChangePasswordRequest
      {
        NewPassword = newUser.Password
      });

      await _context.Users.AddAsync(newUser);
      await _context.SaveChangesAsync();

      var response = new AddUserResponse
      {
        Username = newUser.Username,
        Password = newPassword
      };

      return OLabObjectResult<AddUserResponse>.Result(response);
    }

  }
}