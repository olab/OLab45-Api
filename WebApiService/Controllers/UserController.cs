using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLabWebAPI.Common;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Utils;
using System;
using System.Security.Cryptography;
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
        public IActionResult AddUsers()
        {
            logger.LogDebug($"AddUsers()");

            // test if user has access to add users.
            UserContext userContext = new UserContext(logger, context, HttpContext);
            if (!userContext.HasAccess("X", "UserAdmin", 0))
                return OLabUnauthorizedResult.Result();

            HttpRequest httpRequest = HttpContext.Request;
            Microsoft.Extensions.Primitives.StringValues postedFile = httpRequest.Form["File"];

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
            UserContext userContext = new UserContext(logger, context, HttpContext);
            if (!userContext.HasAccess("X", "UserAdmin", 0))
                return OLabUnauthorizedResult.Result();

            Users user = _userService.GetByUserName(model.Username);
            if (user != null)
                throw new Exception($"User {model.Username} already exists");

            Users newUser = Users.CreateDefault(model);
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

            return OLabObjectResult<AddUserResponse>.Result(response);
        }

    }
}