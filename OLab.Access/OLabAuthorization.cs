using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OLab.Access;

public class OLabAuthorization : IOLabAuthorization
{
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  private readonly GroupReaderWriter _groupReaderWriter;
  private readonly RoleReaderWriter _roleReaderWriter;

  public IUserContext UserContext { get; set; }
  protected IList<GrouproleAcls> _groupRoleAcls = new List<GrouproleAcls>();
  protected IList<UserAcls> _userAcls = new List<UserAcls>();
  public Users OLabUser;

  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";

  public OLabDBContext GetDbContext() { return _dbContext; }
  public IOLabLogger GetLogger() { return _logger; }

  public OLabAuthorization(
    IOLabLogger logger,
    OLabDBContext dbContext
  )
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _logger = logger;
    _dbContext = dbContext;
    _groupReaderWriter = GroupReaderWriter.Instance(logger, dbContext);
    _roleReaderWriter = RoleReaderWriter.Instance(logger, dbContext);
  }

  public void ApplyUserContext(IUserContext userContext)
  {
    Guard.Argument(userContext).NotNull(nameof(userContext));

    UserContext = userContext;

    var userName = UserContext.UserName;
    var userId = UserContext.UserId;

    // load all the user's group/roles acl records
    foreach (var userGroups in UserContext.GroupRoles.Select(x => x.Group).Distinct())
    {
      var groupsPhys = GrouproleAcls.FindByGroup(
        _dbContext,
        userGroups.Name);

      _groupRoleAcls.AddRange(groupsPhys);
    }

    // if local user, read the user-level acls
    var user = GetDbContext().Users.FirstOrDefault(x => x.Username == userName && x.Id == userId);
    if (user != null)
    {
      _logger.LogInformation($"Local user '{userName}' found");

      OLabUser = user;
      userId = user.Id;
      _userAcls = GetDbContext().UserAcls.Where(x => x.UserId == userId).ToList();

      // if user is anonymous user, add user access to anon-flagged maps
      if (OLabUser.Username == Users.AnonymousUserName)
      {
        var anonymousMaps = GetDbContext().Maps.Where(x => x.SecurityId == 1).ToList();
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

  public async Task<IActionResult> HasAccessAsync(
    ulong requestedAcl,
    ScopedObjectDto dto)
  {
    // test if user has access to write to parent.
    if (dto.ImageableType == Constants.ScopeLevelMap)
      if (!await HasAccessAsync(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelMap,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelServer)
      if (!await HasAccessAsync(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelServer,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    if (dto.ImageableType == Constants.ScopeLevelNode)
      if (!await HasAccessAsync(
        IOLabAuthorization.AclBitMaskWrite,
        Constants.ScopeLevelNode,
        dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    return new NoContentResult();
  }

  /// <summary>
  /// Test if have requested access to operation
  /// </summary>
  /// <param name="requestedPerm">Request permissions (bitmask)</param>
  /// <param name="operationType">Operation type</param>
  /// <returns>true/false</returns>
  public bool HasAccess(ulong requestedPerm, string operationType)
  {
    // get group role acl records specific to user
    var groupRoleAcls = GroupRoleAclReaderWriter
      .Instance(GetLogger(), GetDbContext()).GetForUser(UserContext.GroupRoles);

    // test for explicit non-access to specific object type and id
    return groupRoleAcls.Any(x =>
     x.ImageableType == operationType &&
     ( x.Acl2 & requestedPerm ) == requestedPerm);

  }

  /// <summary>
  /// Test if have requested access to securable object
  /// </summary>
  /// <param name="requestedAcl">Request permissions (bitmask)</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  public async Task<bool> HasAccessAsync(
    ulong requestedAcl,
    string objectType,
    uint? objectId)
  {
    Guard.Argument(objectType, nameof(objectType)).NotEmpty();

    var rc = HasUserLevelAccess(requestedAcl, objectType, objectId);
    if (!rc)
    {
      // get group ids that apply to securable object and test if user
      // has access to any group in the list
      var groupIds = await GetSecurableObjectIdsAsync(objectType, objectId);
      foreach (var groupId in groupIds)
      {
        if (HasRoleLevelAccess(requestedAcl, objectType, objectId, groupId))
        {
          rc = true;
          break;
        }
      }
    }

    return rc;
  }

  /// <summary>
  /// Test if have single role-level ACL access
  /// </summary>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <param name="objectGroupId">(optional) group id to evaluate against</param>
  /// <returns>true/false</returns>
  private bool HasRoleLevelAccess(
    ulong requestedPerm,
    string objectType,
    uint? objectId,
    uint objectGroupId)
  {
    // get group role acl records specific to user
    var groupRoleAcls = GroupRoleAclReaderWriter
      .Instance(GetLogger(), GetDbContext()).GetForUser(UserContext.GroupRoles);

    // test for explicit non-access to specific object type and id
    if (groupRoleAcls.Any(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.GroupId == objectGroupId &&
     x.Acl2 == IOLabAuthorization.AclBitMaskNoAccess))
      return false;

    // test for specific object type and id
    if (groupRoleAcls.Any(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.GroupId == objectGroupId &&
     (x.Acl2 & requestedPerm) == requestedPerm))
      return true;

    // test for specific object type and all ids
    if (groupRoleAcls.Any(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     x.GroupId == objectGroupId &&
     (x.Acl2 & requestedPerm) == requestedPerm))
      return true;

    // test for default any object, any id
    if (groupRoleAcls.Any(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == WildCardObjectId &&
     x.GroupId == objectGroupId &&
     (x.Acl2 & requestedPerm) == requestedPerm))
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
  /// Get applicable group id's for an object
  /// </summary>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">Securable object id</param>
  /// <returns>List of group ids</returns>
  private async Task<IList<uint>> GetSecurableObjectIdsAsync(string objectType, uint? objectId)
  {
    var groupIds = new List<uint>();

    if (objectId.HasValue)
    {
      if (objectType == Constants.ScopeLevelNode)
      {
        var nodePhys = await GetDbContext().MapNodes.FirstOrDefaultAsync(x => x.Id == objectId);
        if (nodePhys != null)
        {
          var mapPhys = await GetDbContext().Maps.Include("MapGroups").FirstOrDefaultAsync(x => x.Id == objectId);
          if (mapPhys != null)
            groupIds.AddRange(mapPhys.MapGroups.Select(x => x.GroupId).Distinct());
        }
      }

      else if (objectType == Constants.ScopeLevelMap)
      {
        var mapPhys = await GetDbContext().Maps.Include("MapGroups").FirstOrDefaultAsync(x => x.Id == objectId);
        if (mapPhys != null)
          groupIds.AddRange(mapPhys.MapGroups.Select(x => x.GroupId).Distinct());

      }
    }

    return groupIds;
  }

  /// <summary>
  /// Test if user is system superuser 
  /// </summary>
  /// <returns>true/false</returns>
  public async Task<bool> IsSystemSuperuserAsync()
  {
    return await IsSuperUserInGroup(Groups.OLabGroup);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupName">Group name to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsSuperUserInGroup(string groupName)
  {
    var groupPhys = await _groupReaderWriter.GetAsync(groupName);
    if (groupPhys == null)
    {
      _logger.LogError($"group '{groupName}' not defined.");
      return false;
    }

    return await IsSuperUserInGroup(groupPhys.Id);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupId">Group id to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsSuperUserInGroup(uint groupId)
  {
    var superUserRolePhys = await _roleReaderWriter.GetAsync(Roles.SuperUserRole);
    if (superUserRolePhys == null)
    {
      _logger.LogError($"system role {Roles.SuperUserRole} not defined.");
      return false;
    }

    return UserContext.GroupRoles.Any(x => (x.GroupId == groupId) && (x.RoleId == superUserRolePhys.Id));
  }
}