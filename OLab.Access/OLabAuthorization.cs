using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OLab.Access;

public class OLabAuthorization : IOLabAuthorization
{
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  private readonly GroupRoleReaderWriter _groupRoleReaderWriter;

  public IUserContext UserContext { get; set; }
  protected IList<GrouproleAcls> _roleAcls = new List<GrouproleAcls>();
  protected IList<UserAcls> _userAcls = new List<UserAcls>();
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
    _groupRoleReaderWriter = GroupRoleReaderWriter.Instance(logger, dbContext);
  }

  public void ApplyUserContext(IUserContext userContext)
  {
    Guard.Argument(userContext).NotNull(nameof(userContext));

    UserContext = userContext;

    var userName = UserContext.UserName;
    var userId = UserContext.UserId;

    foreach (var groupRole in UserContext.GroupRoles)
    {
      var groupRolePhys = GrouproleAcls.Find(
        _dbContext,
        groupRole.Group.Name,
        groupRole.Role.Name);

      if (groupRolePhys != null)
        _roleAcls.Add(groupRolePhys);
    }

    // test for a local user
    var user = _dbContext.Users.FirstOrDefault(x => x.Username == userName && x.Id == userId);
    if (user != null)
    {
      _logger.LogInformation($"Local user '{userName}' found");

      OLabUser = user;
      userId = user.Id;
      _userAcls = _dbContext.UserAcls.Where(x => x.UserId == userId).ToList();

      // if user is anonymous user, add user access to anon-flagged maps
      if (OLabUser.Username == Users.AnonymousUserName)
      {
        var anonymousMaps = _dbContext.Maps.Where(x => x.SecurityId == 1).ToList();
        foreach (var item in anonymousMaps)
          _userAcls.Add(new UserAcls
          {
            Id = item.Id,
            ImageableId = item.Id,
            ImageableType = Constants.ScopeLevelMap,
            Acl2 =
              IOLabAuthorization.AclBitMaskRead |
              IOLabAuthorization.AclBitMaskExecute
          });
      }
    }
  }

  public IActionResult HasAccess(ulong requestedAcl, ScopedObjectDto dto)
  {
    // test if user has access to write to parent.
    if (dto.ImageableType == Constants.ScopeLevelMap)
      if (!HasAccess(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelMap,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelServer)
      if (!HasAccess(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelServer,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelNode)
      if (!HasAccess(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelNode,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    return new NoContentResult();
  }

  /// <summary>
  /// Test if have requested access to securable object
  /// </summary>
  /// <param name="requestedAcl">Request permissions (bitmask)</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  public bool HasAccess(
    ulong requestedAcl,
    string objectType,
    uint? objectId)
  {
    Guard.Argument(objectType, nameof(objectType)).NotEmpty();

    var rc = HasUserLevelAccess(requestedAcl, objectType, objectId);
    if (!rc)
      rc = HasRoleLevelAccess(requestedAcl, objectType, objectId);

    return rc;
  }

  /// <summary>
  /// Test if have single role-level ACL access
  /// </summary>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasRoleLevelAccess(
    ulong requestedPerm,
    string objectType,
    uint? objectId)
  {
    // test for explicit non-access to specific object type and id
    var acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl2 == IOLabAuthorization.AclBitMaskNoAccess).FirstOrDefault();

    if (acl != null)
      return true;

    // test for specific object type and id
    acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    // test for specific object type and all ids
    acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    // test for default any object, any id
    acl = _roleAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == WildCardObjectId &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

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
  private bool HasUserLevelAccess(
    ulong requestedPerm,
    string objectType,
    uint? objectId)
  {

    // test for explicit non-access to specific object type and id
    var acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl2 == IOLabAuthorization.AclBitMaskNoAccess).FirstOrDefault();

    if (acl != null)
      return false;

    // test for most specific object acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    // test for specific object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    // test for all for object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == 0 &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    // test for generic acl
    acl = _userAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == 0 &&
     (x.Acl2 & requestedPerm) == requestedPerm).FirstOrDefault();

    if (acl != null)
      return true;

    return false;
  }

  /// <summary>
  /// Test if user is system superuser 
  /// </summary>
  /// <returns>true/false</returns>
  public async Task<bool> IsSystemSuperuserAsync()
  {
    var olabGroupPhys = await _groupRoleReaderWriter.GetGroupAsync(Groups.OLabGroup);
    if (olabGroupPhys == null)
    {
      _logger.LogError($"system group {Groups.OLabGroup} not defined.");
      return false;
    }

    return await IsGroupSuperuserAsync(olabGroupPhys.Id);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupName">Group name to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperuserAsync(string groupName)
  {
    var groupPhys = await _groupRoleReaderWriter.GetGroupAsync(groupName);
    if (groupPhys == null)
    {
      _logger.LogError($"group {groupName} not defined.");
      return false;
    }

    var rolePhys = await _groupRoleReaderWriter.GetRoleAsync(Roles.SuperUserRole);
    if (rolePhys == null)
    {
      _logger.LogError($"system role {Roles.SuperUserRole} not defined.");
      return false;
    }

    return UserContext.GroupRoles.Any(x => (x.GroupId == groupPhys.Id) && (x.RoleId == rolePhys.Id));
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupId">Group id to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperuserAsync(uint groupId)
  {
    var superUserRolePhys = await _groupRoleReaderWriter.GetRoleAsync(Roles.SuperUserRole);
    if (superUserRolePhys == null)
    {
      _logger.LogError($"system role {Roles.SuperUserRole} not defined.");
      return false;
    }

    return UserContext.GroupRoles.Any(x => (x.GroupId == groupId) && (x.RoleId == superUserRolePhys.Id));
  }
}