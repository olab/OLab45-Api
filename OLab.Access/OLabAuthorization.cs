using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OLab.Access
{
  public class OLabAuthorization : IOLabAuthorization
  {
    private readonly IOLabLogger _logger;
    private readonly OLabDBContext _dbContext;
    public IUserContext UserContext { get; set; }
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();
    public Users OLabUser;

    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;
    public const string NonAccessAcl = "-";

    public OLabAuthorization(
      IOLabLogger logger,
      OLabDBContext dbContext
    )
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));

      _logger = logger;
      _dbContext = dbContext;

      //SetUserContext(_userContext.UserRoles, _userContext.UserName, _userContext.UserId);
    }

    public void SetUserContext(IUserContext userContext)
    {
      UserContext = userContext;

      var roles = UserContext.UserRoles;
      string userName = UserContext.UserName;
      uint userId = UserContext.UserId;

      _roleAcls = _dbContext.SecurityRoles.Where(x => roles.Contains(x.Name.ToLower())).ToList();

      // test for a local user
      var user = _dbContext.Users.FirstOrDefault(x => x.Username == userName && x.Id == userId);
      if (user != null)
      {
        _logger.LogInformation($"Local user '{userName}' found");

        OLabUser = user;
        userId = user.Id;
        _userAcls = _dbContext.SecurityUsers.Where(x => x.UserId == userId).ToList();

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
    }

    public IActionResult HasAccess(string acl, ScopedObjectDto dto)
    {
      // test if user has access to write to parent.
      if (dto.ImageableType == Constants.ScopeLevelMap)
        if (!HasAccess("W", Constants.ScopeLevelMap, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      if (dto.ImageableType == Constants.ScopeLevelServer)
        if (!HasAccess("W", Constants.ScopeLevelServer, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      if (dto.ImageableType == Constants.ScopeLevelNode)
        if (!HasAccess("W", Constants.ScopeLevelNode, dto.ImageableId))
          return OLabUnauthorizedResult.Result();

      return new NoContentResult();
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