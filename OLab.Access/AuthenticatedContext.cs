using Dawn;
using Newtonsoft.Json;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OLab.Access;

/// <summary>
/// Represents the user context within the OLab application.
/// </summary>
public abstract class AuthenticatedContext : IAuthenticatedContext
{
  public const string HEADER_SESSIONID = "olabsessionid";
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";
  public Users OLabUser;

  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  protected IList<GrouproleAcls> _roleAcls = [];
  protected IList<UserAcls> _userAcls = [];

  /// <summary>
  /// Gets the database context.
  /// </summary>
  /// <returns>The database context.</returns>
  protected OLabDBContext GetDbContext() { return _dbContext; }

  /// <summary>
  /// Gets the logger.
  /// </summary>
  /// <returns>The logger.</returns>
  protected IOLabLogger GetLogger() { return _logger; }

  private IList<UserGrouproles> _groupRoles = [];
  private uint _userId;
  private string _userName;
  private string _ipAddress;
  private string _issuer;
  private string _courseName;
  private string _sessionId;
  private string _appName;

  /// <summary>
  /// Gets the course name.
  /// </summary>
  public string CourseName { get { return _courseName; } }

  private readonly IDictionary<string, string> _claims = new Dictionary<string, string>();

  /// <summary>
  /// Gets the claims.
  /// </summary>
  public IDictionary<string, string> Claims
  {
    get { return _claims; }
  }

  /// <summary>
  /// Gets or sets the session ID.
  /// </summary>
  public string SessionId
  {
    get => _sessionId;
    set => _sessionId = value;
  }

  /// <summary>
  /// Gets or sets the referring course.
  /// </summary>
  public string ReferringCourse
  {
    get => _courseName;
    set => _courseName = value;
  }

  /// <summary>
  /// Gets or sets the group roles.
  /// </summary>
  public IList<UserGrouproles> GroupRoles
  {
    get => _groupRoles;
    set => _groupRoles = value;
  }

  /// <summary>
  /// Gets or sets the user ID.
  /// </summary>
  public uint UserId
  {
    get => _userId;
    set => _userId = value;
  }

  /// <summary>
  /// Gets or sets the application name.
  /// </summary>
  public string AppName
  {
    get => _appName;
    set => _appName = value;
  }

  /// <summary>
  /// Gets or sets the user name.
  /// </summary>
  public string UserName
  {
    get => _userName;
    set => _userName = value;
  }

  /// <summary>
  /// Gets or sets the IP address.
  /// </summary>
  public string IPAddress
  {
    get => _ipAddress;
    set => _ipAddress = value;
  }

  /// <summary>
  /// Gets or sets the issuer.
  /// </summary>
  public string Issuer
  {
    get => _issuer;
    set => _issuer = value;
  }

  string IAuthenticatedContext.SessionId
  {
    get => _sessionId;
    set => _sessionId = value;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AuthenticatedContext"/> class.
  /// </summary>
  public AuthenticatedContext()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AuthenticatedContext"/> class with the specified logger and database context.
  /// </summary>
  /// <param name="logger">The logger.</param>
  /// <param name="dbContext">The database context.</param>
  public AuthenticatedContext(
    IOLabLogger logger,
    OLabDBContext dbContext)
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );
    Guard.Argument( dbContext ).NotNull( nameof( dbContext ) );

    _logger = logger;
    _dbContext = dbContext;

    GetLogger().LogInformation( $"UserContext ctor" );
  }

  /// <summary>
  /// Sets the claims.
  /// </summary>
  /// <param name="claims">The claims to set.</param>
  protected void SetClaims(IDictionary<string, string> claims)
  {
    foreach ( var claim in claims )
      _claims.Add( claim.Key.ToLower(), claim.Value );

    //GetLogger().LogInformation( $"found {_claims.Count} claims" );
    //foreach ( var claim in _claims )
    //  _logger.LogInformation( $"  header: {claim.Key} = {claim.Value}" );
  }

  /// <summary>
  /// Gets the claim value for the specified key.
  /// </summary>
  /// <param name="key">The key of the claim.</param>
  /// <param name="isRequired">Indicates whether the claim is required.</param>
  /// <returns>The claim value.</returns>
  /// <exception cref="Exception">Thrown if the claim is required and not found.</exception>
  protected string GetClaim(string key, bool isRequired = true)
  {
    if ( _claims.TryGetValue( key.ToLower(), out var value ) )
      return value;

    if ( isRequired )
      throw new Exception( $"claim value '{key}' does not exist" );

    return string.Empty;
  }

  /// <summary>
  /// Loads the user context.
  /// </summary>
  /// <exception cref="Exception">Thrown if no headers are found.</exception>
  protected void LoadUserContext()
  {
    UserName = GetClaim( ClaimTypes.Name, false );
    if ( string.IsNullOrEmpty( UserName ) )
      UserName = GetClaim( "name" );

    ReferringCourse = "olabinternal";
    ReferringCourse = GetClaim( ClaimTypes.UserData, false );
    Issuer = GetClaim( "iss" );
    UserId = (uint)Convert.ToInt32( GetClaim( "id" ) );
    AppName = GetClaim( "app" );

    var groupRoleString = GetClaim( ClaimTypes.Role, false );
    if ( string.IsNullOrEmpty( groupRoleString ) )
      groupRoleString = GetClaim( "role" );

    GroupRoles = UserGrouproles.StringToObjectList( GetDbContext(), groupRoleString );
  }

  public static string TruncateToJsonObject(IAuthenticatedContext phys, int maxDepth)
  {
    var json = JsonConvert.SerializeObject(
      new List<IAuthenticatedContext> { phys },
      new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore } );

    return SerializerUtilities.TruncateJsonToDepth( json, maxDepth + 1 );
  }

  /// <summary>
  /// Returns a string that represents the current object.
  /// </summary>
  /// <returns>A string that represents the current object.</returns>
  public override string ToString()
  {
    return $"{UserId} {Issuer} {UserName} {UserGrouproles.ListToString( GroupRoles )} {IPAddress} {ReferringCourse}";
  }
}
