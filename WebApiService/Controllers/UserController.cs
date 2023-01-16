using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Data;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi
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
      byte[] randomNumber = new byte[32];
      using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
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
      AuthenticateResponse response = _userService.Authenticate(
        new LoginRequest
        {
          Username = model.Username,
          Password = model.Password
        });

      if (response == null)
        return BadRequest(new { message = "Username or password is incorrect" });

      Users user = _userService.GetByUserName(model.Username);
      _userService.ChangePassword(user, model);

      dbContext.Users.Update(user);

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

      AuthenticateResponse response = _userService.Authenticate(model);
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

      AuthenticateResponse response = _userService.AuthenticateExternal(model);
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
    public async Task<IActionResult> AddUsers(IFormFile file)
    {
      logger.LogDebug($"AddUsers()");

      try
      {

        var responses = new List<AddUserResponse>();

        // test if user has access to add users.
        var userContext = new UserContext(logger, dbContext, HttpContext);
        if (!userContext.HasAccess("X", "UserAdmin", 0))
          return OLabUnauthorizedResult.Result();

        var result = new List<string>();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
          while (reader.Peek() >= 0)
          {
            var userRequestText = reader.ReadLine();
            var userRequest = new AddUserRequest(userRequestText);

            var response = await ProcessUserRequest(userRequest);
            responses.Add(response);
          }
        }

        return OLabObjectListResult<AddUserResponse>.Result(responses);

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
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddUser([FromBody] JArray jsonStringData)
    {
      try
      {
        var responses = new List<AddUserResponse>();

        var items = JsonConvert.DeserializeObject<List<AddUserRequest>>(jsonStringData.ToString());

        logger.LogDebug($"AddUser(items count '{items.Count}')");

        // test if user has access to add users.
        UserContext userContext = new UserContext(logger, dbContext, HttpContext);
        if (!userContext.HasAccess("X", "UserAdmin", 0))
          return OLabUnauthorizedResult.Result();

        foreach (var item in items)
        {
          var response = await ProcessUserRequest(item);
          responses.Add(response);
        }

        return OLabObjectListResult<AddUserResponse>.Result(responses);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    private async Task<AddUserResponse> ProcessUserRequest(AddUserRequest userRequest)
    {
      Users user = _userService.GetByUserName(userRequest.Username);
      if (user != null)
        throw new Exception($"User {userRequest.Username} already exists");

      Users newUser = Users.CreateDefault(userRequest);
      string newPassword = newUser.Password;

      _userService.ChangePassword(newUser, new ChangePasswordRequest
      {
        NewPassword = newUser.Password
      });

      await _context.Users.AddAsync(newUser);
      await _context.SaveChangesAsync();

      AddUserResponse response = new AddUserResponse
      {
        Username = newUser.Username,
        Password = newPassword
      };

      return response;
    }

  }
}