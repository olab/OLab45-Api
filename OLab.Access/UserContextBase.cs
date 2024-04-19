using Dawn;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Access;
public abstract class UserContextBase : IUserContext
{
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";
  public ClaimsPrincipal User;
  public Users OLabUser;

  protected IDictionary<string, string> _claims;
  protected readonly IOLabLogger _logger;
  private readonly OLabDBContext dbContext;
  protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
  protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

  protected string _sessionId;
  protected string _role;
  public IList<UserGroups> UserRoles { get; set; }
  protected uint _userId;
  protected string _userName;
  protected string _ipAddress;
  protected string _issuer;
  protected string _referringCourse;
  protected string _accessToken;
  protected readonly string _courseName;

  public string SessionId
  {
    get => _sessionId;
    set => _sessionId = value;
  }

  public string ReferringCourse
  {
    get => _referringCourse;
    set => _referringCourse = value;
  }

  public string Role
  {
    get => _role;
    set => _role = value;
  }

  public uint UserId
  {
    get => _userId;
    set => _userId = value;
  }

  public string UserName
  {
    get => _userName;
    set => _userName = value;
  }

  public string IPAddress
  {
    get => _ipAddress;
    set => _ipAddress = value;
  }

  public string Issuer
  {
    get => _issuer;
    set => _issuer = value;
  }

  public string CourseName { get { return null; } }

  // default ctor, needed for services Dependancy Injection
  public UserContextBase()
  {

  }

  public UserContextBase(
    IOLabLogger logger,
    OLabDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _logger = logger;
    this.dbContext = dbContext;

    _logger.LogInformation($"UserContextBase ctor");

  }

  /// <summary>
  /// Test if user member of group/role
  /// </summary>
  /// <param name="groupName">Group name (or *)</param>
  /// <param name="RoleName">Role name (or *)</param>
  /// <returns></returns>
  public bool IsMemberOf(string groupName, string RoleName)
  {
    foreach (var item in UserRoles)
    {
      if ((groupName == "*") && (RoleName != "*"))
      {
        if (item.Role.Name == RoleName)
          return true;
      }

      if ((groupName != "*") && (RoleName == "*"))
      {
        if (item.Group.Name == groupName)
          return true;
      }

      if ((item.Group.Name == groupName) && (item.Role.Name == RoleName))
        return true;
    }

    return false;
  }

  public override string ToString()
  {
    return $"{UserId} {Issuer} {UserName} {Role} {IPAddress} {ReferringCourse}";
  }
}
