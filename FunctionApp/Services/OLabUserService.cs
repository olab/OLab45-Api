using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dawn;
using Microsoft.AspNetCore.Http;
using OLabWebAPI.Common.Exceptions;
using JWT.Algorithms;
using JWT;
using JWT.Serializers;

namespace OLab.FunctionApp.Api.Services
{
  public class OLabUserService : IUserService
  {
    public static int defaultTokenExpiryMinutes = 120;
    private readonly AppSettings _appSettings;
    private readonly OLabDBContext _context;
    private readonly OLabLogger _logger;
    private readonly IList<Users> _users;
    private static TokenValidationParameters _tokenParameters;
    private IEnumerable<Claim> _claims;

    public bool IsValid { get; private set; }
    public bool UserName { get; private set; }
    public bool Role { get; private set; }

    private readonly IJwtAlgorithm _algorithm;
    private readonly IJsonSerializer _serializer;
    private readonly IBase64UrlEncoder _base64Encoder;
    private readonly IJwtEncoder _jwtEncoder;

    public OLabUserService(
      ILogger<OLabUserService> logger,
      IOptions<AppSettings> appSettings,
      OLabDBContext context)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(appSettings).NotNull(nameof(appSettings));
      Guard.Argument(context).NotNull(nameof(context));

      defaultTokenExpiryMinutes = OLabConfiguration.DefaultTokenExpiryMins;
      _appSettings = appSettings.Value;
      _context = context;
      _logger = new OLabLogger(logger);

      _users = _context.Users.OrderBy(x => x.Id).ToList();

      _logger.LogDebug($"appSetting aud: '{_appSettings.Audience}', secret: '{_appSettings.Secret[..4]}...'");

      _tokenParameters = SetupValidationParameters(_appSettings);

      // JWT specific initialization.
      _algorithm = new HMACSHA256Algorithm();
      _serializer = new JsonNetSerializer();
      _base64Encoder = new JwtBase64UrlEncoder();
      _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);
    }


    /// <summary>
    /// Extract claims from token
    /// </summary>
    /// <param name="token">Bearer token</param>
    private static IEnumerable<Claim> ExtractTokenClaims(string token)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
      return securityToken.Claims;
    }

    /// <summary>
    /// Build token validation object
    /// </summary>
    /// <param name="appSettings">Application settings</param>
    /// <returns>TokenValidationParameters object</returns>
    private static TokenValidationParameters SetupValidationParameters(AppSettings appSettings)
    {
      Guard.Argument(appSettings, nameof(appSettings)).NotNull();

      var jwtIssuer = "moodle";
      var jwtAudience = appSettings.Audience;
      var signingSecret = appSettings.Secret;

      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(signingSecret[..16]));

      var tokenParameters = new TokenValidationParameters
      {
        ValidateIssuer = false,
        ValidIssuers = new List<string> { jwtIssuer, appSettings.Issuer },

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,

        // validate against existing security key
        IssuerSigningKey = securityKey
      };

      return tokenParameters;

    }

    /// <summary>
    /// Authenticate user
    /// </summary>
    /// <param name="model">Login model</param>
    /// <returns>Authenticate response, or null</returns>
    public AuthenticateResponse Authenticate(LoginRequest model)
    {
      Guard.Argument(model, nameof(model)).NotNull();

      var user = _users.SingleOrDefault(x => x.Username == model.Username);

      // return null if user not found
      if (user != null)
      {
        if (ValidatePassword(model.Password, user))
        {
          // authentication successful so generate jwt token
          return IssueJWT(user);
        }
      }

      return null;
    }

    /// <summary>
    /// Updates a user record with a new password
    /// </summary>
    /// <param name="user">Existing user record from DB</param>
    /// <param name="model">Change password request model</param>
    /// <returns></returns>
    public void ChangePassword(Users user, ChangePasswordRequest model)
    {
      Guard.Argument(user, nameof(user)).NotNull();
      Guard.Argument(model, nameof(model)).NotNull();

      var clearText = model.NewPassword;

      // add password salt, if it's defined
      if (!string.IsNullOrEmpty(user.Salt))
        clearText += user.Salt;

      SHA1 hash = SHA1.Create();
      byte[] plainTextBytes = Encoding.ASCII.GetBytes(clearText);
      byte[] hashBytes = hash.ComputeHash(plainTextBytes);

      user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Get all defined users
    /// </summary>
    /// <returns>Enumerable list of users</returns>
    public IEnumerable<Users> GetAll()
    {
      return _users;
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    /// <param name="id">User id</param>
    /// <returns>User record</returns>
    public Users GetById(int id)
    {
      return _users.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Get user by name
    /// </summary>
    /// <param name="userName">User name</param>
    /// <returns>User record</returns>
    public Users GetByUserName(string userName)
    {
      return _users.FirstOrDefault(x => x.Username == userName);
    }

    /// <summary>
    /// Validate user password
    /// </summary>
    /// <param name="clearText">Password</param>
    /// <param name="user">Corresponding user record</param>
    /// <returns>true/false</returns>
    public static bool ValidatePassword(string clearText, Users user)
    {
      Guard.Argument(user, nameof(user)).NotNull();
      Guard.Argument(clearText, nameof(clearText)).NotEmpty();

      if (!string.IsNullOrEmpty(user.Salt))
      {
        clearText += user.Salt;
        SHA1 hash = SHA1.Create();
        byte[] plainTextBytes = Encoding.ASCII.GetBytes(clearText);
        byte[] hashBytes = hash.ComputeHash(plainTextBytes);
        string localChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return localChecksum == user.Password;
      }

      return false;
    }

    /// <summary>
    /// Generate an epoch token expiry time
    /// </summary>
    /// <param name="numberOfDays">Number of days to add to current date</param>
    /// <returns>Epoch time</returns>
    private static int GenerateTokenExpiry(int numberOfDays)
    {
      Guard.Argument(numberOfDays, nameof(numberOfDays)).NotZero();

      TimeSpan t = DateTime.UtcNow.AddDays(7) - new DateTime(1970, 1, 1);
      var expiryInSeconds = (int)t.TotalSeconds;
      return expiryInSeconds;
    }

    /// <summary>
    /// ISsue the JWT token
    /// </summary>
    /// <param name="user">User to issue for</param>
    /// <returns>AuthenticateResponse</returns>
    public AuthenticateResponse IssueJWT(Users user)
    {
      Guard.Argument(user, nameof(user)).NotNull();

      TimeSpan t = DateTime.UtcNow.AddDays(7) - new DateTime(1970, 1, 1);
      var expiryInSeconds = GenerateTokenExpiry(7);

      Dictionary<string, object> claims = new()
      {
        // JSON representation of the user Reference with ID and display name
        { "name", user.Username },
        { "role", user.Role },
        { "exp", expiryInSeconds },
        { "iss", _appSettings.Issuer },
        { "aud", _appSettings.Audience }
      };

      string securityToken = _jwtEncoder.Encode(claims, _appSettings.Secret[..16]);

      var response = new AuthenticateResponse();
      response.AuthInfo.Token = securityToken;
      response.AuthInfo.Refresh = null;
      response.Role = $"{user.Role}";
      response.UserName = user.Username;
      response.AuthInfo.Created = DateTime.UtcNow;
      response.AuthInfo.Expires =
        response.AuthInfo.Created.AddMinutes(defaultTokenExpiryMinutes);

      return response;
    }

    /// <summary>
    /// validate token/setup up common properties
    /// </summary>
    /// <param name="request">HTTP request</param>
    public void ValidateToken(HttpRequest request)
    {
      Guard.Argument(request, nameof(request)).NotNull();

      var bearerToken = AccessTokenUtils.ExtractBearerToken( request );

      // Extract/save token claims
      _claims = ExtractTokenClaims(bearerToken);

      try
      {
        var handler = new JwtSecurityTokenHandler();

        // Try to validate the token. Throws if the 
        // token cannot be validated.
        handler.ValidateToken(
            bearerToken,
            _tokenParameters,
            out _); // Discard the output SecurityToken. We don't need it.      
      }
      catch (Exception ex)
      {
        _logger.LogError($"ValidateToken error {ex.Message}");
        throw new OLabUnauthorizedException();
      }

    }

    public AuthenticateResponse AuthenticateExternal(ExternalLoginRequest model)
    {
      throw new NotImplementedException();
    }

    public void AddUser(Users newUser)
    {
      throw new NotImplementedException();
    }
  }
}