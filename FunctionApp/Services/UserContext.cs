using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

#nullable disable

namespace OLab.FunctionApp.Services
{
  public class UserContext : IUserContext
  {
    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;
    public const string NonAccessAcl = "-";
    public ClaimsPrincipal Principal;
    public Users OLabUser;

    private readonly OLabDBContext _dbContext;
    private readonly FunctionContext _hostContext;

    private IDictionary<string, string> _claims;
    protected readonly OLabLogger _logger;
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

    private IOLabSession _session;
    private string _role;
    private IList<string> _roles;
    private uint _userId;
    private string _userName;
    private string _ipAddress;
    private string _issuer;
    //private readonly string _courseName;
    //private string _accessToken;

    public IOLabSession Session
    {
      get => _session;
      set => _session = value;
    }

    public string ReferringCourse
    {
      get => _role;
      set => _role = value;
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

    public string SessionId { get { return Session.GetSessionId(); } }

    //public string CourseName { get { return _courseName; } }
    public string CourseName { get { return null; } }

    // default ctor, needed for services Dependancy Injection
    public UserContext()
    {

    }

    public UserContext(OLabLogger logger, OLabDBContext dbContext, FunctionContext hostContext)
    {
      _dbContext = dbContext;
      _logger = logger;
      _hostContext = hostContext;

      Session = new OLabSession(_logger.GetLogger(), dbContext, this);

      LoadHostContext();
    }

    protected virtual void LoadHostContext()
    {
      var headers = new Dictionary<string, string>();
      if (!_hostContext.Items.TryGetValue("headers", out var headersObjects))
        throw new Exception("unable to retrieve headers from host context");

      headers = (Dictionary<string, string>)headersObjects;

      if (headers.TryGetValue("OLabSessionId", out var sessionId))
      {
        if (!string.IsNullOrEmpty(sessionId) && sessionId != "null")
        {
          Session.SetSessionId(sessionId);
          if (!string.IsNullOrWhiteSpace(Session.GetSessionId()))
            _logger.LogInformation($"Found sessionId {Session.GetSessionId()}.");
        }
      }

      //var identity = (ClaimsIdentity)_httpContext.User.Identity;
      //if (identity == null)
      //  throw new Exception($"Unable to establish identity from token");

      //Principal = _httpContext.User;

      if (!_hostContext.Items.TryGetValue("claims", out object claimsObject))
        throw new Exception("unable to retrieve claims from host context");

      _claims = (IDictionary<string, string>)claimsObject;

      if (!_claims.TryGetValue(ClaimTypes.Name, out var nameValue))
        throw new Exception("unable to retrieve user name from token claims");
      UserName = nameValue;

      ReferringCourse = _claims[ClaimTypes.UserData];

      if (!_claims.TryGetValue("iss", out var issValue))
        throw new Exception("unable to retrieve iss from token claims");
      Issuer = issValue;

      if (!_claims.TryGetValue("id", out var idValue))
        throw new Exception("unable to retrieve user id from token claims");
      UserId = (uint)Convert.ToInt32(idValue);

      if (!_claims.TryGetValue("id", out var roleValue))
        throw new Exception("unable to retrieve role from token claims");
      var Role = roleValue;

      // separate out multiple roles, make lower case, remove spaces, and sort
      _roles = Role.Split(',')
        .Select(x => x.Trim())
        .Select(x => x.ToLower())
        .OrderBy(x => x)
        .ToList();

      _roleAcls = _dbContext.SecurityRoles.Where(x => _roles.Contains(x.Name.ToLower())).ToList();

      if (headers.TryGetValue("x-forwarded-for", out _ipAddress))
        _logger.LogInformation($"ipaddress: {_ipAddress}");
      else
        _logger.LogWarning($"no ipaddress detected");

      // test for a local user
      var user = _dbContext.Users.FirstOrDefault(x => x.Username == UserName && x.Id == UserId);
      if (user != null)
      {
        _logger.LogInformation($"Local user '{UserName}' found");

        OLabUser = user;
        UserId = user.Id;
        _userAcls = _dbContext.SecurityUsers.Where(x => x.UserId == UserId).ToList();

        // if user is anonymous user, add user access to anon-flagged maps
        if (OLabUser.Group == "anonymous")
        {
          var anonymousMaps = _dbContext.Maps.Where(x => x.SecurityId == 1).ToList();
          foreach (var item in anonymousMaps)
            _userAcls.Add(new SecurityUsers
            {
              Id = item.Id,
              ImageableId = item.Id,
              ImageableType = Constants.ScopeLevelMap,
              Acl = "RX"
            });
        }
      }
      else
        throw new Exception($"no user found for {UserName}({UserId})");
    }

    /// <summary>
    /// Test if have requested access to securable object
    /// </summary>
    /// <param name="requestedPerm">Request permissions (RWED)</param>
    /// <param name="objectType">Securable object type</param>
    /// <param name="objectId">(optional) securable object id</param>
    /// <returns>true/false</returns>
    public bool HasAccess(string requestedPerm, string objectType, uint? objectId)
    {
      var grantedCount = 0;

      if (!objectId.HasValue)
        objectId = WildCardObjectId;

      for (var i = 0; i < requestedPerm.Length; i++)
        if (HasSingleAccess(requestedPerm[i], objectType, objectId))
          grantedCount++;

      return grantedCount == requestedPerm.Length;
    }

    /// <summary>
    /// Test if have single ACL access
    /// </summary>
    /// <param name="requestedPerm">Single-letter ACL to test for</param>
    /// <param name="objectType">Securable object type</param>
    /// <param name="objectId">(optional) securable object id</param>
    /// <returns>true/false</returns>
    private bool HasSingleAccess(char requestedPerm, string objectType, uint? objectId)
    {
      var rc = HasUserLevelAccess(requestedPerm, objectType, objectId);
      if (!rc)
        rc = HasRoleLevelAccess(requestedPerm, objectType, objectId);

      return rc;
    }

    /// <summary>
    /// Test if have single role-level ACL access
    /// </summary>
    /// <param name="requestedPerm">Single-letter ACL to test for</param>
    /// <param name="objectType">Securable object type</param>
    /// <param name="objectId">(optional) securable object id</param>
    /// <returns>true/false</returns>
    private bool HasRoleLevelAccess(char requestedPerm, string objectType, uint? objectId)
    {
      // test for explicit non-access to specific object type and id
      var acl = _roleAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == objectId.Value &&
       x.Acl == NonAccessAcl).FirstOrDefault();

      if (acl != null)
        return true;

      // test for specific object type and id
      acl = _roleAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == objectId.Value &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for specific object type and all ids
      acl = _roleAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == WildCardObjectId &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for default any object, any id
      acl = _roleAcls.Where(x =>
       x.ImageableType == WildCardObjectType &&
       x.ImageableId == WildCardObjectId &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      return false;
    }

    /// <summary>
    /// Test if have single user-level ACL access
    /// </summary>
    /// <param name="requestedPerm">Single-letter ACL to test for</param>
    /// <param name="objectType">Securable object type</param>
    /// <param name="objectId">(optional) securable object id</param>
    /// <returns>true/false</returns>
    private bool HasUserLevelAccess(char requestedPerm, string objectType, uint? objectId)
    {

      // test for explicit non-access to specific object type and id
      var acl = _userAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == objectId.Value &&
       x.Acl == NonAccessAcl).FirstOrDefault();

      if (acl != null)
        return false;

      // test for most specific object acl
      acl = _userAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == objectId.Value &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for specific object type acl
      acl = _userAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == WildCardObjectId &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for all for object type acl
      acl = _userAcls.Where(x =>
       x.ImageableType == objectType &&
       x.ImageableId == 0 &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for generic acl
      acl = _userAcls.Where(x =>
       x.ImageableType == WildCardObjectType &&
       x.ImageableId == 0 &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      return false;
    }
  }
}

