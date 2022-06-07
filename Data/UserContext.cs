using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using OLabWebAPI.Data.Session;

#nullable disable

namespace OLabWebAPI.Model
{
  public class UserContext
  {
    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;

    private HttpContext _httpContext;
    private IEnumerable<Claim> _claims;
    private ClaimsPrincipal _user;
    private readonly OLabDBContext _dbContext;
    private readonly OLabLogger _logger;
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();
    
    public readonly IOLabSession Session;
    public string SessionId { get; set; }
    public string Role { get; protected set; }
    public uint UserId { get; protected set; }
    public string UserName { get; protected set; }
    public string IPAddress { get; protected set; }

    public UserContext(OLabLogger logger, OLabDBContext dbContext)
    {
      _dbContext = dbContext;
      _logger = logger;
      Session = new OLabSession(_logger.GetLogger(), dbContext);
    }

    public UserContext(OLabLogger logger, OLabDBContext context, HttpContext httpContext)
    {
      _dbContext = context;
      _logger = logger;
      Session = new OLabSession(_logger.GetLogger(), context);
    
      LoadHttpContext(httpContext);
    }

    protected virtual void LoadHttpContext(HttpContext httpContext)
    {
      _httpContext = httpContext;

      SessionId = _httpContext.Request.Headers["OLabSessionId"].FirstOrDefault();
      if ( !string.IsNullOrWhiteSpace( SessionId ))
        _logger.LogInformation($"Found SessionId {SessionId}.");

      IPAddress = httpContext.Connection.RemoteIpAddress.ToString();

      var identity = (ClaimsIdentity)_httpContext.User.Identity;
      if (identity == null)
        throw new Exception($"Unable to establish identity from token");

      _user = httpContext.User;
      _claims = identity.Claims;

      UserName = httpContext.Items["User"].ToString();

      var user = _dbContext.Users.FirstOrDefault(x => x.Username == UserName);
      if (user != null)
      {
        UserId = user.Id;
        Role = httpContext.Items["Role"].ToString();

        _roleAcls = _dbContext.SecurityRoles.Where(x => x.Name.ToLower() == Role.ToLower()).ToList();
        _userAcls = _dbContext.SecurityUsers.Where(x => x.UserId == UserId).ToList();
      }
      else
        _logger.LogWarning($"User {UserName} does not exist");

    }

    /// <summary>
    /// Test if have requested access to securable object
    /// </summary>
    /// <param name="httpContext">HttpContext (when not known at CTOR time)</param>
    /// <param name="requestedPerm">Request permissions (RWED)</param>
    /// <param name="objectType">Securable object type</param>
    /// <param name="objectId">(optional) securable object id</param>
    /// <returns>true/false</returns>
    public bool HasAccess(HttpContext httpContext, string requestedPerm, string objectType, uint? objectId = null)
    {
      LoadHttpContext(httpContext);
      return HasAccess(requestedPerm, objectType, objectId);
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
      int grantedCount = 0;

      for (int i = 0; i < requestedPerm.Length; i++)
      {
        if (HasSingleAccess(requestedPerm[i], objectType, objectId))
          grantedCount++;
        else
          _logger.LogError($"User {UserId}/{Role} does not have '{requestedPerm[i]}' access to '{objectType}({objectId})'");
      }

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
      if (!objectId.HasValue)
        objectId = 0;

      bool rc = HasUserLevelAccess(requestedPerm, objectType, objectId);
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
      // test for most specific object acl
      var acl = _roleAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for specific object type acl
      acl = _roleAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == WildCardObjectId) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for generic acl
      acl = _roleAcls.Where(x =>
       (x.ImageableType == WildCardObjectType) &&
       (x.ImageableId == 0) &&
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

      // test for most specific object acl
      var acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for specific object type acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == WildCardObjectId) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      // test for generic acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == WildCardObjectType) &&
       (x.ImageableId == 0) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
        return true;

      return false;
    }
  }
}

