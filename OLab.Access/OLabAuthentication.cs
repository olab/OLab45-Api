using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using OLab.Access.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;
using OLab.Api.Common.Exceptions;
using System.Reflection.PortableExecutable;

namespace OLab.Access;

public class OLabAuthentication : IOLabAuthentication
{
  public static int defaultTokenExpiryMinutes = 120;
  private static IOLabConfiguration _config;
  private readonly IOLabLogger Logger;
  private readonly TokenValidationParameters _tokenParameters;

  /// <summary>
  /// Retreive Claims dictionary
  /// </summary>
  public IDictionary<string, string> Claims { get; private set; }

  public OLabAuthentication(
    ILoggerFactory loggerFactory,
    IOLabConfiguration config)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(config).NotNull(nameof(config));

    _config = config;

    defaultTokenExpiryMinutes = _config.GetAppSettings().TokenExpiryMinutes;

    Logger = OLabLogger.CreateNew<OLabAuthentication>(loggerFactory);

    Logger.LogInformation($"Authorization ctor");
    Logger.LogInformation($"appSetting aud: '{_config.GetAppSettings().Audience}', secret: '{_config.GetAppSettings().Secret[..4]}...'");

    _tokenParameters = BuildTokenValidationObject();
  }
  public TokenValidationParameters GetValidationParameters() { return _tokenParameters; }

  /// <summary>
  /// Builds token validation object
  /// </summary>
  /// <param name="configuration">App configuration</param>
  private TokenValidationParameters BuildTokenValidationObject()
  {
    try
    {

      // get and extract the valid token issuers
      var jwtIssuers = _config.GetValue<string>("Issuer");

      var issuerParts = jwtIssuers.Split(',');
      var validIssuers = issuerParts.Select(x => x.Trim()).ToList();

      var jwtAudience = _config.GetValue<string>("Audience");

      var signingSecret = _config.GetValue<string>("Secret");
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
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw;
    }

  }

  /// <summary>
  /// Gets the access token from the request headers
  /// </summary>
  /// <param name="headers">Request headers</param>
  /// <param name="allowAnonymous">Flag if not no token allowed</param>
  /// <returns>token</returns>
  /// <exception cref="OLabUnauthorizedException"></exception>
  public virtual string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers, 
    IReadOnlyDictionary<string, object> bindingData)
  {
    Guard.Argument(headers).NotNull(nameof(headers));

    var token = string.Empty;

    // handler for external logins
    if (bindingData.TryGetValue("token", out var externalToken))
    {
      token = externalToken as string;
      Logger.LogInformation("Binding data token provided");
    }

    // handler for signalR logins 
    else if (bindingData.TryGetValue("access_token", out var signalRToken))
    {
      token = signalRToken as string;
      Logger.LogInformation("Signalr token provided");
    }


    else if (headers.TryGetValue("authorization", out var authHeader))
    {
      if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      {
        token = authHeader.Substring("Bearer ".Length).Trim();
        Logger.LogInformation("Authorization header bearer token provided");
      }
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
      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      var claimsPrincipal = tokenHandler.ValidateToken(
        token,
        _tokenParameters,
        out var validatedToken);

      Claims = new Dictionary<string, string>();
      foreach (var claim in claimsPrincipal.Claims)
        Claims.Add(claim.Type, claim.Value);

      return true;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw new OLabUnauthorizedException();
    }
  }

  /// <summary>
  /// Generate JWT token
  /// </summary>
  /// <param name="user">User record from database</param>
  /// <returns>AuthenticateResponse</returns>
  /// <remarks>https://duyhale.medium.com/generate-short-lived-symmetric-jwt-using-microsoft-identitymodel-d9c2478d2d5a</remarks>
  public AuthenticateResponse GenerateJwtToken(Users user, string issuedBy = "olab")
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
}
