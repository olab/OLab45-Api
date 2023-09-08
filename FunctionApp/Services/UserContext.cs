using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using System.Security.Claims;

#nullable disable

namespace OLab.FunctionApp.Services
{
  public abstract class UserContext : IUserContext
  {
    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;
    public const string NonAccessAcl = "-";
    public ClaimsPrincipal User;
    public Users OLabUser;

    protected IDictionary<string, string> _claims;
    protected readonly OLabDBContext _dbContext;
    protected readonly OLabLogger _logger;
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

    protected IOLabSession _session;
    protected string _role;
    protected IList<string> _roles;
    protected uint _userId;
    protected string _userName;
    protected string _ipAddress;
    protected string _issuer;
    protected string _referringCourse;
    protected string _accessToken;

    public IOLabSession Session
    {
      get => _session;
      set => _session = value;
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

    public string SessionId { get { return Session.GetSessionId(); } }

    //public string CourseName { get { return _courseName; } }
    public string CourseName { get { return null; } }

    protected abstract void LoadHostContext();

    // default ctor, needed for services Dependancy Injection
    public UserContext()
    {

    }

    public UserContext(OLabLogger logger, OLabDBContext dbContext)
    {
      _dbContext = dbContext;
      _logger = logger;
      Session = new OLabSession(_logger.GetLogger(), dbContext, this);
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

