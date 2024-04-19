using Dawn;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OLab.Access;

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
  }

  public void ApplyUserContext(IUserContext userContext)
  {
    Guard.Argument(userContext).NotNull(nameof(userContext));

    UserContext = userContext;

    var userRoles = UserContext.UserRoles;
    var userName = UserContext.UserName;
    var userId = UserContext.UserId;

    foreach (var userRole in userRoles)
      _roleAcls.AddRange(SecurityRoles.GetAcls(_dbContext, userRole));

    // test for a local user
    var user = _dbContext.Users.FirstOrDefault(x => x.Username == userName && x.Id == userId);
    if (user != null)
    {
      _logger.LogInformation($"Local user '{userName}' found");

      OLabUser = user;

      // read user-specific object ACL's
      _userAcls = SecurityUsers.GetAcls(_dbContext, user.Id);

      // if user is anonymous user, add user access to anon-flagged maps
      if (OLabUser.UserGroups.Select(x => x.Group.Name).Contains("anonymous"))
      {
        var anonymousMaps = _dbContext.Maps.Where(x => x.SecurityId == 1).ToList();
        foreach (var item in anonymousMaps)
          _userAcls.Add(new SecurityUsers
          {
            Id = item.Id,
            ImageableId = item.Id,
            ImageableType = Constants.ScopeLevelMap,
            Acl2 = SecurityRoles.Read | SecurityRoles.Execute
          });
      }
    }
  }

  public IActionResult HasAccess(ulong acl, ScopedObjectDto dto)
  {
    // test if user has access to write to parent.
    if (dto.ImageableType == Constants.ScopeLevelMap)
      if (!HasAccess(acl, Constants.ScopeLevelMap, dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelServer)
      if (!HasAccess(acl, Constants.ScopeLevelServer, dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelNode)
    {
      if (!HasAccess(acl, Constants.ScopeLevelNode, dto.ImageableId))
      {
        // if no access at node level, check access to owning map
        var mapNodePhys = _dbContext.MapNodes.FirstOrDefault(x => x.Id == dto.ImageableId);
        if (!HasAccess(acl, Constants.ScopeLevelMap, mapNodePhys.MapId))
          return OLabUnauthorizedResult.Result();
      }
    }

    return new NoContentResult();
  }

  /// <summary>
  /// Test if have requested access to securable object
  /// </summary>
  /// <param name="requestedPerm">ACL bit mask</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  public bool HasAccess(ulong requestedPerm, string objectType, uint? objectId)
  {
    var grantedCount = 0;

    if (!objectId.HasValue)
      objectId = WildCardObjectId;

    // loop through every role for user
    foreach (var rolePhys in UserContext.UserRoles)
    {
      if (HasSingleAccess(rolePhys, requestedPerm, objectType, objectId))
        grantedCount++;
    }


    return grantedCount > 0;
  }

  /// <summary>
  /// Test if have single ACL access
  /// </summary>
  /// <param name="rolePhys">USerGroup context to apply</param>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasSingleAccess(
    UserGroups rolePhys,
    ulong requestedPerm,
    string objectType,
    uint? objectId)
  {
    var rc = HasUserLevelAccess(requestedPerm, objectType, objectId);
    if (!rc)
      rc = HasRoleLevelAccess(rolePhys, requestedPerm, objectType, objectId);

    return rc;
  }

  /// <summary>
  /// Test if have single role-level ACL access
  /// </summary>
  /// <param name="rolePhys">UserGroup context to apply</param>
  /// <param name="requestedPerm">ACL Bit mask</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasRoleLevelAccess(
    UserGroups rolePhys,
    ulong requestedPerm,
    string objectType,
    uint? objectId)
  {
    bool hasAccess = false;

    var userGroupAcls = _roleAcls.Where(x =>
      (x.GroupId == rolePhys.GroupId) &&
      (x.RoleId == rolePhys.RoleId));

    // if no ACL's for the role, then no access within
    // the role context
    if (!userGroupAcls.Any())
      return false;

    // test for explicit access to specific object type and specific id
    var acl = userGroupAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      hasAccess = true;

    // test for explicit access to specific object type and wildcard ids
    acl = userGroupAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      hasAccess = true;

    // test for explicit access to default access to any object and wildcard ids
    acl = userGroupAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == WildCardObjectId &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      hasAccess = true;

    // overriding test for explicit no-access to specific object type and id
    // overrides any previous 'allow' access obtained
    var noAccessAcl = userGroupAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     requestedPerm == SecurityUsers.NoAccess &&
     x.Acl2 == SecurityRoles.NoAccess).FirstOrDefault();

    if (noAccessAcl != null)
      hasAccess = false;

    return hasAccess;
  }

  /// <summary>
  /// Test if have single user-level ACL access
  /// </summary>
  /// <param name="requestedPerm">ACL bit mask</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasUserLevelAccess(ulong requestedPerm, string objectType, uint? objectId)
  {

    // test for explicit non-access to specific object type and id
    var acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl2 == SecurityRoles.NoAccess).FirstOrDefault();

    if (acl != null)
      return false;

    // test for most specific object acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      return true;

    // test for specific object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      return true;

    // test for all for object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == 0 &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      return true;

    // test for generic acl
    acl = _userAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == 0 &&
     ((x.Acl2 & requestedPerm) == requestedPerm)).FirstOrDefault();

    if (acl != null)
      return true;

    return false;
  }

  /// <summary>
  /// Test if user member of specified group/role
  /// </summary>
  /// <param name="groupName">Group name</param>
  /// <param name="roleName">Role name</param>
  /// <returns></returns>
  public bool IsMemberOf(string groupName, string roleName)
  {
    var isMember = UserContext.IsMemberOf(groupName, roleName);

    // if no access, test if OLab Superuser, which overrides the parametered names
    if (!isMember)
      isMember = UserContext.IsMemberOf(Groups.GroupNameOLab, Roles.RoleNameSuperuser);

    return isMember;
  }
}