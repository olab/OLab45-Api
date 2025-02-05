using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using OLab.Access.Interfaces;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Access;

/// <summary>
/// Provides authentication services for OLab, including token generation, validation, and user authentication.
/// </summary>
public class OLabAuthentication : IOLabAuthentication
{
  /// <summary>
  /// Default token expiry time in minutes.
  /// </summary>
  public static int defaultTokenExpiryMinutes = 120;
  private static IOLabConfiguration _config;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabLogger _logger;
  private readonly TokenValidationParameters _tokenParameters;

  public const int SaltLength = 64;

  /// <summary>
  /// Gets the database context.
  /// </summary>
  /// <returns>The OLabDBContext instance.</returns>
  public OLabDBContext GetDbContext() { return _dbContext; }

  /// <summary>
  /// Gets the logger instance.
  /// </summary>
  /// <returns>The IOLabLogger instance.</returns>
  public IOLabLogger GetLogger() { return _logger; }

  /// <summary>
  /// Retrieves the claims dictionary.
  /// </summary>
  public IDictionary<string, string> Claims { get; private set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="OLabAuthentication"/> class.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="config">The configuration instance.</param>
  /// <param name="dbContext">The database context instance.</param>
  public OLabAuthentication(
  IOLabLogger logger,
  IOLabConfiguration config,
  OLabDBContext dbContext) : this( config, dbContext )
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );

    _logger = logger;
    GetLogger().LogInformation( $"OLabAuthentication ctor" );
  }

  private OLabAuthentication(
    IOLabConfiguration config,
    OLabDBContext dbContext)
  {
    Guard.Argument( config ).NotNull( nameof( config ) );
    Guard.Argument( dbContext ).NotNull( nameof( dbContext ) );

    _config = config;
    _dbContext = dbContext;

    defaultTokenExpiryMinutes = _config.GetAppSettings().TokenExpiryMinutes;
    _tokenParameters = BuildTokenValidationObject( _config );
  }

  /// <summary>
  /// Exposes the centralized token validation parameters.
  /// </summary>
  /// <returns>The TokenValidationParameters instance.</returns>
  public TokenValidationParameters GetValidationParameters() { return _tokenParameters; }

  /// <summary>
  /// Builds the token validation object.
  /// </summary>
  /// <param name="config">The configuration instance.</param>
  /// <returns>The TokenValidationParameters instance.</returns>
  public static TokenValidationParameters BuildTokenValidationObject(IOLabConfiguration config)
  {
    // get and extract the valid token issuers
    var jwtIssuers = config.GetAppSettings().Issuer;

    var issuerParts = jwtIssuers.Split( ',' );
    var validIssuers = issuerParts.Select( x => x.Trim() ).ToList();

    var jwtAudience = config.GetAppSettings().Audience;

    var signingSecret = config.GetAppSettings().Secret;
    var secretBytes = Encoding.Default.GetBytes( signingSecret[ ..40 ] );
    var securityKey =
      new SymmetricSecurityKey( secretBytes );

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
  /// Extracts an access token from an HttpRequest.
  /// </summary>
  /// <param name="request">The HttpRequest instance.</param>
  /// <param name="allowAnonymous">Flag indicating if anonymous access is allowed when no token is available.</param>
  /// <returns>The extracted bearer token.</returns>
  /// <exception cref="OLabUnauthorizedException">Thrown when unable to extract authorization token and anonymous access is not allowed.</exception>
  public static string ExtractAccessToken(HttpRequest request, bool allowAnonymous = false)
  {
    var token = "";

    if ( request.Headers.ContainsKey( "Authorization" ) )
    {
      token = request.Headers[ "Authorization" ];
      token = token.Replace( "Bearer ", "" );
    }

    // handler external app posted token
    if ( request.Query.ContainsKey( "token" ) )
      token = request.Query[ "token" ];

    // handler SignalR posted token
    if ( request.Query.ContainsKey( "access_token" ) )
      token = request.Query[ "access_token" ];

    if ( string.IsNullOrEmpty( token ) && !allowAnonymous )
      throw new OLabUnauthorizedException( "Unable to extract authorization token" );

    return token;
  }

  /// <summary>
  /// Gets the access token from request headers and binding data.
  /// </summary>
  /// <param name="headers">The request headers dictionary.</param>
  /// <param name="bindingData">The binding data (optional).</param>
  /// <returns>The extracted bearer token.</returns>
  /// <exception cref="OLabUnauthorizedException">Thrown when no authorization token is provided.</exception>
  public virtual string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers,
    IReadOnlyDictionary<string, object> bindingData = null)
  {
    Guard.Argument( headers ).NotNull( nameof( headers ) );

    GetLogger().LogInformation( "Validating token" );

    var token = string.Empty;

    // handler for external logins
    if ( (bindingData != null) && bindingData.TryGetValue( "token", out var externalToken ) )
    {
      token = externalToken as string;
      GetLogger().LogInformation( "Binding data token provided" );
    }

    // handler for signalR logins 
    else if ( (bindingData != null) && bindingData.TryGetValue( "access_token", out var signalRToken ) )
    {
      token = signalRToken as string;
      GetLogger().LogInformation( "Signalr token provided" );
    }

    // handle Authorization header token
    else if ( headers.TryGetValue( "authorization", out var authHeader ) )
    {
      token = authHeader.Replace( "Bearer ", "" );
      GetLogger().LogInformation( "Authorization header bearer token provided" );
    }

    if ( string.IsNullOrEmpty( token ) )
    {
      GetLogger().LogError( "No auth token provided" );
      throw new OLabUnauthorizedException();
    }

    return token;
  }

  /// <summary>
  /// Validates a token.
  /// </summary>
  /// <param name="token">The bearer token.</param>
  /// <returns>True if the token is valid; otherwise, false.</returns>
  /// <exception cref="OLabUnauthorizedException">Thrown when the token cannot be validated.</exception>
  public virtual bool ValidateToken(string token)
  {
    Guard.Argument( token ).NotEmpty( nameof( token ) );

    try
    {
      token = token.Replace( "Bearer ", "" );

      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      var claimsPrincipal = tokenHandler.ValidateToken(
        token,
        GetValidationParameters(),
        out var validatedToken );

      Claims = new Dictionary<string, string>();

      foreach ( var claim in claimsPrincipal.Claims )
      {
        var added = Claims.TryAdd( claim.Type, claim.Value );
        GetLogger().LogDebug( $" claim: {claim.Type} = {claim.Value}. added: {added}" );
      }

      GetLogger().LogInformation( "Bearer token validated" );

      return true;
    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex.Message );
      throw;
    }
  }

  /// <summary>
  /// Generates a JWT token.
  /// </summary>
  /// <param name="user">The user record from the database.</param>
  /// <param name="referrer">The referrer information.</param>
  /// <param name="issuedBy">The issuer of the token.</param>
  /// <returns>The AuthenticateResponse instance containing the generated token.</returns>
  /// <remarks>https://duyhale.medium.com/generate-short-lived-symmetric-jwt-using-microsoft-identitymodel-d9c2478d2d5a</remarks>
  public AuthenticateResponse GenerateJwtToken(
    Users user,
    string referrer,
    string issuedBy = "olab")
  {
    Guard.Argument( user, nameof( user ) ).NotNull();

    GetLogger().LogDebug( $"generating token" );

    var secretBytes = Encoding.Default.GetBytes( _config.GetAppSettings().Secret[ ..40 ] );

    var securityKey =
      new SymmetricSecurityKey( secretBytes );

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity( new Claim[]
      {
        new Claim(ClaimTypes.Name, user.Username.ToLower()),
        new Claim(ClaimTypes.Role, $"{UserGrouproles.ListToString(user.UserGrouproles.ToList())}"),
        new Claim("name", user.Nickname),
        new Claim("sub", user.Username),
        new Claim("id", $"{user.Id}"),
        new Claim(ClaimTypes.UserData, $"{user.Settings}"),
        new Claim("app", referrer),
      } ),
      Expires = DateTime.UtcNow.AddDays( 7 ),
      Issuer = issuedBy,
      Audience = _config.GetAppSettings().Audience,
      SigningCredentials = new SigningCredentials( securityKey, SecurityAlgorithms.HmacSha256Signature )
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken( tokenDescriptor );
    var securityToken = tokenHandler.WriteToken( token );

    var response = new AuthenticateResponse();
    response.AuthInfo.Token = securityToken;
    response.AuthInfo.Refresh = null;
    response.Role = UserGrouproles.ListToString( user.UserGrouproles.ToList() );
    response.UserName = user.Username;
    response.AuthInfo.Created = DateTime.UtcNow;
    response.AuthInfo.Expires =
      response.AuthInfo.Created.AddMinutes( defaultTokenExpiryMinutes );

    return response;
  }

  /// <summary>
  /// Generates a JWT token for anonymous use.
  /// </summary>
  /// <param name="mapId">The map ID to query.</param>
  /// <returns>The AuthenticateResponse instance containing the generated token.</returns>
  /// <exception cref="Exception">Thrown when no user is defined for anonymous map play or the map is not defined.</exception>
  public async Task<AuthenticateResponse> GenerateAnonymousJwtTokenAsync(uint mapId)
  {
    var physUser = await UserReaderWriter
      .Instance( GetLogger(), GetDbContext() )
      .GetSingleAsync( Users.AnonymousUserName );

    if ( physUser == null )
      throw new Exception( $"No user is defined for anonymous map play" );

    var map = GetDbContext().Maps
      .FirstOrDefault( x => x.Id == mapId );
    if ( map == null )
      throw new Exception( $"Map {mapId} is not defined." );

    // test for 'open' map
    if ( map.SecurityId != 1 )
      GetLogger().LogError( $"Map {mapId} is not configured for anonymous map play" );

    var issuedBy = "olab";

    var authenticateResponse = GenerateJwtToken( physUser, issuedBy );

    return authenticateResponse;
  }

  /// <summary>
  /// Generates a JWT token from an external one.
  /// </summary>
  /// <param name="model">The token payload.</param>
  /// <returns>The AuthenticateResponse instance containing the generated token.</returns>
  public AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model)
  {
    var externalAuth = new OLabAuthentication( _logger, _config, _dbContext );
    externalAuth.ValidateToken( model.ExternalToken );

    GetLogger().LogDebug( $"External JWT Incoming token claims:" );
    foreach ( var claim in externalAuth.Claims )
      GetLogger().LogDebug( $" {claim.Key} = {claim.Value}" );

    var user = new Users();

    if ( externalAuth.Claims.TryGetValue( "unique_name", out var value ) )
    {
      user.Username = value;
      user.Nickname = value;
    }

    if ( externalAuth.Claims.TryGetValue( "role", out value ) )
      user.UserGrouproles.AddRange( UserGrouproles.StringToObjectList( _dbContext, value ) );

    if ( externalAuth.Claims.TryGetValue( "id", out value ) )
      user.Id = (uint)Convert.ToInt32( value );

    if ( externalAuth.Claims.TryGetValue( "course", out value ) )
      user.Settings = value;

    var issuedBy = externalAuth.Claims[ "iss" ];

    var authenticateResponse = GenerateJwtToken( user, issuedBy );

    // add (any) course name to the authenticate response
    authenticateResponse.CourseName = user.Settings;

    return authenticateResponse;
  }

  /// <summary>
  /// Authenticates a user.
  /// </summary>
  /// <param name="model">The login model.</param>
  /// <param name="impersonateMode">Flag indicating if the user is a superuser impersonating another user.</param>
  /// <returns>The authenticated user, or null if authentication fails.</returns>
  public async Task<Users> AuthenticateAsync(LoginRequest model, bool impersonateMode = false)
  {
    Guard.Argument( model, nameof( model ) ).NotNull();

    if ( !impersonateMode )
    {
      if ( model.Password.Length > 3 )
        GetLogger().LogInformation( $"Authenticating {model.Username}, ***{model.Password[ ^3.. ]}" );
      else
        GetLogger().LogInformation( $"Authenticating {model.Username}, ***" );
    }

    var user = await UserReaderWriter
      .Instance( GetLogger(), GetDbContext() )
      .GetSingleAsync( model.Username );

    if ( user != null )
    {
      // check if non-anonymous user
      if ( model.Username.ToLower() != Users.AnonymousUserName.ToLower() )
      {
        if ( !impersonateMode )
        {
          // not impersonating, so check password
          if ( !ValidatePassword( model.Password, user ) )
            return null;
        }
      }
    }

    return user;
  }


  /// <summary>
  /// Validates a user's password.
  /// </summary>
  /// <param name="clearText">The clear text password.</param>
  /// <param name="physUser">The corresponding user record.</param>
  /// <returns>True if the password is valid; otherwise, false.</returns>
  public bool ValidatePassword(string clearText, Users physUser)
  {
    Guard.Argument( physUser, nameof( physUser ) ).NotNull();
    Guard.Argument( clearText, nameof( clearText ) ).NotEmpty();

    var result = false;

    if ( !string.IsNullOrEmpty( physUser.Salt ) )
    {
      clearText += physUser.Salt;
      var hash = SHA1.Create();
      var plainTextBytes = Encoding.ASCII.GetBytes( clearText );
      var hashBytes = hash.ComputeHash( plainTextBytes );
      var localChecksum = BitConverter.ToString( hashBytes ).Replace( "-", "" ).ToLowerInvariant();

      result = localChecksum == physUser.Password;
    }

    GetLogger().LogInformation( $"Password validated = {result}" );
    return result;
  }

  /// <summary>
  /// Updates a user's password.
  /// </summary>
  /// <param name="newPassword">new cleartext passwd</param>
  /// <param name="physUser">Users record</param>
  /// <returns>true, if changed</returns>
  public bool UpdatePassword(string newPassword, Users physUser)
  {
    var result = false;
    Guard.Argument( physUser, nameof( physUser ) ).NotNull();

    if ( string.IsNullOrEmpty( newPassword ) )
      result = false;

    else
    {
      // test if new password same as old one
      var validPassword = ValidatePassword( newPassword, physUser );

      if ( !validPassword )
      {
        physUser.Salt = StringUtils.GenerateRandomString( SaltLength );
        var clearText = newPassword + physUser.Salt;

        using ( var hash = SHA1.Create() )
        {
          var plainTextBytes = Encoding.ASCII.GetBytes( clearText );
          var hashBytes = hash.ComputeHash( plainTextBytes );
          var encryptedPassword
            = BitConverter.ToString( hashBytes ).Replace( "-", "" ).ToLowerInvariant();

          physUser.Password = encryptedPassword;
          result = true;
        }
      }

    }

    GetLogger().LogInformation( $"Password changed for '{physUser.Username}'? {result}" );

    return result;
  }

}
