using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OLab.Access.Interfaces;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace OLab.Access;

public class OLabAuthentication : IOLabAuthentication
{
  public static int defaultTokenExpiryMinutes = 120;
  private static IOLabConfiguration _config;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabLogger Logger;
  private readonly TokenValidationParameters _tokenParameters;

  /// <summary>
  /// Retreive Claims dictionary
  /// </summary>
  public IDictionary<string, string> Claims { get; private set; }

  private OLabAuthentication(
    IOLabConfiguration config,
    OLabDBContext dbContext)
  {
    Guard.Argument(config).NotNull(nameof(config));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _config = config;
    _dbContext = dbContext;

    defaultTokenExpiryMinutes = _config.GetAppSettings().TokenExpiryMinutes;
    _tokenParameters = BuildTokenValidationObject(_config);
  }

  public OLabAuthentication(
    IOLabLogger logger,
    IOLabConfiguration config,
    OLabDBContext dbContext) : this(config, dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));

    Logger = logger;
    Logger.LogInformation($"Authorization ctor");
    Logger.LogInformation($"appSetting aud: '{_config.GetAppSettings().Audience}', secret: '{_config.GetAppSettings().Secret[..4]}...'");
  }

  /// <summary>
  /// Expose the centralized token validation parameters
  /// </summary>
  /// <returns>TokenValidationParameters</returns>
  public TokenValidationParameters GetValidationParameters() { return _tokenParameters; }

  /// <summary>
  /// Builds token validation object
  /// </summary>
  /// <param name="configuration">App cfg</param>
  public static TokenValidationParameters BuildTokenValidationObject(IOLabConfiguration config)
  {
    // get and extract the valid token issuers
    var jwtIssuers = config.GetValue<string>("Issuer");

    var issuerParts = jwtIssuers.Split(',');
    var validIssuers = issuerParts.Select(x => x.Trim()).ToList();

    var jwtAudience = config.GetValue<string>("Audience");

    var signingSecret = config.GetValue<string>("Secret");
    var secretBytes = Encoding.Default.GetBytes(signingSecret[..40]);
    var securityKey =
      new SymmetricSecurityKey(secretBytes);

    var tokenParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuers = validIssuers,
      ValidateIssuerSigningKey = true,

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
  /// Extract an access token from a HttpRequest
  /// </summary>
  /// <param name="request">HttpRequest</param>
  /// <param name="allowAnonymous">Flag is anonymous is allowed when no token available</param>
  /// <returns>Bearer token</returns>
  /// <exception cref="OLabUnauthorizedException"></exception>
  public string ExtractAccessToken(HttpRequest request, bool allowAnonymous = false)
  {
    var token = "";

    if (request.Headers.ContainsKey("Authorization"))
    {
      token = request.Headers["Authorization"];
      token = token.Replace("Bearer ", "");
    }

    // handler external app posted token
    if (request.Query.ContainsKey("token"))
      token = request.Query["token"];

    // handler SignalR posted token
    if (request.Query.ContainsKey("access_token"))
      token = request.Query["access_token"];

    if (string.IsNullOrEmpty(token) && !allowAnonymous)
    {
      Logger.LogError("Unable to extract authorization token");
      throw new OLabUnauthorizedException();
    }

    return token;
  }

  /// <summary>
  /// Gets the access token from request headers and binding Data
  /// </summary>
  /// <param name="headers">Request headers dictionary</param>
  /// <param name="bindingData">Binding data (optional)</param>
  /// <returns>Bearer token</returns>
  /// <exception cref="OLabUnauthorizedException"></exception>
  public virtual string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers,
    IReadOnlyDictionary<string, object> bindingData = null)
  {
    Guard.Argument(headers).NotNull(nameof(headers));

    Logger.LogInformation("Validating token");

    var token = string.Empty;


    // handler for external logins
    if ((bindingData != null) && bindingData.TryGetValue("token", out var externalToken))
    {
      token = externalToken as string;
      Logger.LogInformation("Binding data token provided");
    }

    // handler for signalR logins 
    else if ((bindingData != null) && bindingData.TryGetValue("access_token", out var signalRToken))
    {
      token = signalRToken as string;
      Logger.LogInformation("Signalr token provided");
    }

    // handle Authorization header token
    else if (headers.TryGetValue("authorization", out var authHeader))
    {
      token = authHeader.Replace("Bearer ", "");
      Logger.LogInformation("Authorization header bearer token provided");
    }

    if (string.IsNullOrEmpty(token))
    {
      Logger.LogError("No auth token provided");
      throw new OLabUnauthorizedException();
    }

    return token;
  }

  /// <summary>
  /// Validates a token
  /// </summary>
  /// <param name="token">Bearer token</param>
  /// <returns>true, if success</returns>
  /// <exception cref="OLabUnauthorizedException"></exception>
  public virtual bool ValidateToken(string token)
  {
    Guard.Argument(token).NotEmpty(nameof(token));

    try
    {
      token = token.Replace("Bearer ", "");

      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      var claimsPrincipal = tokenHandler.ValidateToken(
        token,
        GetValidationParameters(),
        out var validatedToken);

      Claims = new Dictionary<string, string>();

      foreach (var claim in claimsPrincipal.Claims)
      {
        var added = Claims.TryAdd(claim.Type, claim.Value);
        Logger.LogInformation($" claim: {claim.Type} = {claim.Value}. added: {added}");
      }

      Logger.LogInformation("bearer token validated");

      return true;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw;
    }
  }

  /// <summary>
  /// Generate JWT token
  /// </summary>
  /// <param name="user">User record from database</param>
  /// <returns>AuthenticateResponse</returns>
  /// <remarks>https://duyhale.medium.com/generate-short-lived-symmetric-jwt-using-microsoft-identitymodel-d9c2478d2d5a</remarks>
  public AuthenticateResponse GenerateJwtToken(
    Users user,
    string issuedBy = "olab")
  {
    Guard.Argument(user, nameof(user)).NotNull();

    Logger.LogDebug($"generatring token");

    var securityKey =
      new SymmetricSecurityKey(Encoding.Default.GetBytes(_config.GetAppSettings().Secret[..40]));

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new Claim[]
      {
        new Claim(ClaimTypes.Name, user.Username.ToLower()),
        new Claim(ClaimTypes.Role, $"{user.Role}"),
        new Claim("name", user.Nickname),
        new Claim("sub", user.Username),
        new Claim("id", $"{user.Id}"),
        new Claim(ClaimTypes.UserData, $"{user.Settings}")
      }),
      Expires = DateTime.UtcNow.AddDays(7),
      Issuer = issuedBy,
      Audience = _config.GetAppSettings().Audience,
      SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var securityToken = tokenHandler.WriteToken(token);

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
  /// Generate JWT token for anonymous use
  /// </summary>
  /// <param name="mapId">map id to query</param>
  /// <returns>AuthenticateResponse</returns>
  public AuthenticateResponse GenerateAnonymousJwtToken(uint mapId)
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

    var authenticateResponse = GenerateJwtToken(user, issuedBy);

    return authenticateResponse;
  }


  /// <summary>
  /// Generate JWT token from external one
  /// </summary>
  /// <param name="model">token payload</param>
  /// <returns>AuthenticateResponse</returns>
  public AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model)
  {
    var externalAuth = new OLabAuthentication(Logger, _config, _dbContext);
    externalAuth.ValidateToken(model.ExternalToken);

    Logger.LogDebug($"External JWT Incoming token claims:");
    foreach (var claim in externalAuth.Claims)
      Logger.LogDebug($" {claim.Key} = {claim.Value}");

    var user = new Users();

    if (externalAuth.Claims.TryGetValue("unique_name", out var value))
    {
      user.Username = value;
      user.Nickname = value;
    }

    if (externalAuth.Claims.TryGetValue("role", out value))
      user.Role = value;

    if (externalAuth.Claims.TryGetValue("id", out value))
      user.Id = (uint)Convert.ToInt32(value);

    if (externalAuth.Claims.TryGetValue("course", out value))
      user.Settings = value;

    var issuedBy = externalAuth.Claims["iss"];

    var authenticateResponse = GenerateJwtToken(user, issuedBy);

    // add (any) course name to the authenticate response
    authenticateResponse.CourseName = user.Settings;

    return authenticateResponse;
  }
}
