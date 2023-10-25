using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Security.Cryptography;
using System.Text;

namespace OLab.FunctionApp.Services;

public class UserService : IUserService
{
  public static int defaultTokenExpiryMinutes = 120;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabAuthentication _authentication;
  private readonly OLabAuthentication _externalAuth;
  private readonly IOLabLogger Logger;

  public bool IsValid { get; private set; }
  public bool UserName { get; private set; }
  public bool Role { get; private set; }

  public UserService(
    ILoggerFactory loggerFactory,
    OLabDBContext context,
    IOLabAuthentication authentication,
    IOLabConfiguration config)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(context).NotNull(nameof(context));
    Guard.Argument(authentication).NotNull(nameof(authentication));
    Guard.Argument(config).NotNull(nameof(config));

    _dbContext = context;
    _authentication = authentication;
    _externalAuth = new OLabAuthentication(loggerFactory, config);
    
    defaultTokenExpiryMinutes = config.GetAppSettings().TokenExpiryMinutes;

    Logger = OLabLogger.CreateNew<UserService>(loggerFactory);

    Logger.LogInformation($"UserService ctor");
    Logger.LogInformation($"appSetting aud: '{config.GetAppSettings().Audience}', secret: '{config.GetAppSettings().Secret[..4]}...'");

  }

  /// <summary>
  /// Authenticate anonymously
  /// </summary>
  /// <param name="model">Login model</param>
  /// <returns>Authenticate response, or null</returns>
  public AuthenticateResponse AuthenticateAnonymously(uint mapId)
  {
    return GenerateAnonymousJwtToken(mapId);
  }

  /// <summary>
  /// Authenticate external-issued token
  /// </summary>
  /// <param name="model">Login model</param>
  /// <returns>Authenticate response, or null</returns>
  public AuthenticateResponse AuthenticateExternal(ExternalLoginRequest model)
  {
    return GenerateExternalJwtToken(model);
  }

  /// <summary>
  /// Authenticate user
  /// </summary>
  /// <param name="model">Login model</param>
  /// <returns>Authenticate response, or null</returns>
  public AuthenticateResponse Authenticate(LoginRequest model)
  {
    Guard.Argument(model, nameof(model)).NotNull();

    Logger.LogInformation($"Authenticating {model.Username}, ***{model.Password[^3..]}");
    var user = _dbContext.Users.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower());

    // return null if user not found
    if (user != null)
      if (ValidatePassword(model.Password, user))
        // _authentication successful so generate jwt token
        return _authentication.GenerateJwtToken(user);

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

    var hash = SHA1.Create();
    var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
    var hashBytes = hash.ComputeHash(plainTextBytes);

    user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }

  /// <summary>
  /// Get all defined users
  /// </summary>
  /// <returns>Enumerable list of users</returns>
  public IEnumerable<Users> GetAll()
  {
    return _dbContext.Users.ToList();
  }

  /// <summary>
  /// Get user by Id
  /// </summary>
  /// <param name="id">User id</param>
  /// <returns>User record</returns>
  public Users GetById(int id)
  {
    return _dbContext.Users.FirstOrDefault(x => x.Id == id);
  }

  /// <summary>
  /// Get user by name
  /// </summary>
  /// <param name="userName">User name</param>
  /// <returns>User record</returns>
  public Users GetByUserName(string userName)
  {
    return _dbContext.Users.FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
  }

  /// <summary>
  /// Validate user password
  /// </summary>
  /// <param name="clearText">Password</param>
  /// <param name="user">Corresponding user record</param>
  /// <returns>true/false</returns>
  public bool ValidatePassword(string clearText, Users user)
  {
    Guard.Argument(user, nameof(user)).NotNull();
    Guard.Argument(clearText, nameof(clearText)).NotEmpty();

    var result = false;

    if (!string.IsNullOrEmpty(user.Salt))
    {
      clearText += user.Salt;
      var hash = SHA1.Create();
      var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
      var hashBytes = hash.ComputeHash(plainTextBytes);
      var localChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

      result = localChecksum == user.Password;
    }

    Logger.LogInformation($"Password validated = {result}");
    return result;
  }

  /// <summary>
  /// Generate JWT token for anonymous use
  /// </summary>
  /// <param name="mapId">map id to query</param>
  /// <returns>AuthenticateResponse</returns>
  private AuthenticateResponse GenerateAnonymousJwtToken(uint mapId)
  {
    // get user flagged for anonymous use
    var serverUser = _dbContext.Users.FirstOrDefault(x => x.Group == "anonymous");
    if (serverUser == null)
      throw new Exception($"No user is defined for anonymous map play");

    var map = _dbContext.Maps.FirstOrDefault(x => x.Id == mapId);
    if (map == null)
      throw new Exception($"Map {mapId} is not defined.");

    // test for 'open' map
    if (map.SecurityId != 1)
      Logger.LogError($"Map {mapId} is not configured for anonymous map play");

    var user = new Users();

    user.Username = serverUser.Username;
    user.Role = serverUser.Role;
    user.Nickname = serverUser.Nickname;
    user.Id = serverUser.Id;
    var issuedBy = "olab";

    var authenticateResponse = _authentication.GenerateJwtToken(user, issuedBy);

    return authenticateResponse;
  }


  /// <summary>
  /// Generate JWT token from external one
  /// </summary>
  /// <param name="model">token payload</param>
  /// <returns>AuthenticateResponse</returns>
  private AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model)
  {
    _externalAuth.ValidateToken( model.ExternalToken );

    Logger.LogDebug($"External JWT Incoming token claims:");
    foreach (var claim in _externalAuth.Claims)
      Logger.LogDebug($" {claim.Key} = {claim.Value}");

    var user = new Users();

    if (_externalAuth.Claims.TryGetValue("unique_name", out string value))
    {
      user.Username = value;
      user.Nickname = value;
    }

    if (_externalAuth.Claims.TryGetValue("role", out value))
      user.Role = value;

    if (_externalAuth.Claims.TryGetValue("id", out value))
      user.Id = (uint)Convert.ToInt32(value);

    if (_externalAuth.Claims.TryGetValue("course", out value))
      user.Settings = value;

    var issuedBy = _externalAuth.Claims["iss"];

    var authenticateResponse = _authentication.GenerateJwtToken(user, issuedBy);

    // add (any) course name to the authenticate response
    authenticateResponse.CourseName = user.Settings;

    return authenticateResponse;
  }

  public void AddUser(Users newUser)
  {
    throw new NotImplementedException();
  }

}