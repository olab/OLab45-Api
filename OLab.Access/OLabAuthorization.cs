using Dawn;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OLab.Api.Common;
using OLab.Api.Data.Exceptions;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OLab.Access;

public class OLabAuthorization : IOLabAuthorization
{
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  private readonly GroupReaderWriter _groupReaderWriter;
  private readonly RoleReaderWriter _roleReaderWriter;

  public IUserContext UserContext { get; set; }
  protected IList<GrouproleAcls> _groupRoleAcls = new List<GrouproleAcls>();
  protected IList<UserGrouproles> _userGroupRoles = new List<UserGrouproles>();
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

  /// <summary>
  /// Add user Authorization and load group/role acls
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  public void ApplyUserContext(Users userPhys)
  {
    Guard.Argument(userPhys).NotNull(nameof(userPhys));

    OLabUser = userPhys;
    _userGroupRoles = OLabUser.UserGrouproles.ToList();

    // load all the user's group/roles acl records
    foreach (var userGroups in _userGroupRoles.Select(x => x.Group).Distinct())
    {
      var groupsPhys = GrouproleAcls.FindByGroup(
        _dbContext,
        userGroups.Name);

      _groupRoleAcls.AddRange(groupsPhys);

      // add default no-group acls
      groupsPhys = GrouproleAcls.FindByGroup(
        _dbContext,
        string.Empty);

      _groupRoleAcls.AddRange(groupsPhys);

    }
  }

  /// <summary>
  /// Add user context to Authorization and load group/role acls
  /// </summary>
  /// <param name="userContext">User context</param>
  public void ApplyUserContext(IUserContext userContext)
  {
    Guard.Argument(userContext).NotNull(nameof(userContext));

    UserContext = userContext;
    _userGroupRoles = UserContext.GroupRoles.ToList();

    var userName = UserContext.UserName;
    var userId = UserContext.UserId;

    // load all the user's group/roles acl records
    foreach (var userGroups in _userGroupRoles.Select(x => x.Group).Distinct())
    {
      var groupsPhys = GrouproleAcls.FindByGroup(
        _dbContext,
        userGroups.Name);

      _groupRoleAcls.AddRange(groupsPhys);

      // add default no-group acls
      groupsPhys = GrouproleAcls.FindByGroup(
        _dbContext,
        string.Empty);

      _groupRoleAcls.AddRange(groupsPhys);

    }

    // if local user, read the user-level acls
    //var user = GetDbContext().Users.FirstOrDefault(x => x.Username == userName && x.Id == userId);
    //if (user != null)
    //{
    //  _logger.LogInformation($"Local user '{userName}' found");

    //  OLabUser = user;
    //  userId = user.Id;
    //  _userAcls = GetDbContext().UserAcls.Where(x => x.UserId == userId).ToList();

    //  // if user is anonymous user, add user access to anon-flagged maps
    //  if (OLabUser.Username == Users.AnonymousUserName)
    //  {
    //    var anonymousMaps = GetDbContext().Maps.Where(x => x.SecurityId == 1).ToList();
    //    foreach (var item in anonymousMaps)
    //      _userAcls.Add(new UserAcls
    //      {
    //        Id = item.Id,
    //        ImageableId = item.Id,
    //        ImageableType = Constants.ScopeLevelMap,
    //        Acl2 =
    //          IOLabAuthorization.AclBitMaskRead |
    //          IOLabAuthorization.AclBitMaskExecute
    //      });
    //  }
    //}
  }

  /// <summary>
  /// Test if user is system superuser 
  /// </summary>
  /// <returns>true/false</returns>
  public async Task<bool> IsSystemSuperuserAsync()
  {
    return await IsGroupSuperUser(Groups.OLabGroup);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupName">Group name to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUser(string groupName)
  {
    var groupPhys = await _groupReaderWriter.GetAsync(groupName);
    if (groupPhys == null)
    {
      _logger.LogError($"group '{groupName}' not defined.");
      return false;
    }

    return await IsGroupSuperUser(groupPhys.Id);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupId">Group id to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUser(uint groupId)
  {
    var superUserRolePhys = await _roleReaderWriter.GetAsync(Roles.SuperUserRole);
    if (superUserRolePhys == null)
    {
      _logger.LogError($"system role {Roles.SuperUserRole} not defined.");
      return false;
    }

    return _userGroupRoles.Any(x => (x.GroupId == groupId) && (x.RoleId == superUserRolePhys.Id));
  }

  /// <summary>
  /// Test if have access to scoped object
  /// </summary>
  /// <param name="acl"></param>
  /// <param name="dto"></param>
  /// <returns>true/false</returns>
  public async Task<IActionResult> HasAccessAsync(
    ulong requestedAcl,
    ScopedObjectDto dto)
  {
    // test if user has access to parent map.
    if (dto.ImageableType == Constants.ScopeLevelMap)
    {
      bool result = await HasRequestedAccessToMapAsync(requestedAcl, dto.ImageableId);

      if (!result)
        return OLabUnauthorizedResult.Result();
    }


    // test if user has access to parent node.
    if (dto.ImageableType == Constants.ScopeLevelNode)
    {
      bool result = await HasRequestedAccessToNodeAsync(requestedAcl, dto.ImageableId);

      if (!result)
        return OLabUnauthorizedResult.Result();
    }

    return new NoContentResult();
  }

  /// <summary>
  /// Test if group/role has requested access to object
  /// </summary>
  /// <param name="groupId">Group id to search for (null = all)</param>
  /// <param name="roleId">Role id to search for (null = all)</param>
  /// <param name="objectType">Object type to search for (null = all)</param>
  /// <param name="objectId">Object id to search for (null = all)</param>
  /// <param name="requestedAcl">ACL to compare against</param>
  /// <returns>true/false, no acl</returns>
  private async Task<bool?> HasRequestedAccessAsync(
    uint? groupId,
    uint? roleId,
    string objectType,
    uint? objectId,
    ulong requestedAcl)
  {
    // test if system superuser, meaning full access
    if (await IsSystemSuperuserAsync())
      return true;

    if (groupId.HasValue)
    {
      // test if group superuser, meaning full access
      if (await IsGroupSuperUser(groupId.Value))
        return true;
    }

    // test for explicit object type and id specified
    // for specific group and role
    var acl = _groupRoleAcls.FirstOrDefault(x =>
      x.ImageableId == objectId &&
      x.ImageableType == objectType &&
      x.GroupId == groupId &&
      x.RoleId == roleId);

    if (acl != null)
      return (acl.Acl2 & requestedAcl) == requestedAcl;

    // test for explicit object type and id specified
    // for specific group (all roles)
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.ImageableId == objectId &&
      x.ImageableType == objectType &&
      x.GroupId == groupId &&
      x.RoleId == null);

    if (acl != null)
      return (acl.Acl2 & requestedAcl) == requestedAcl;


    // test for any object of type specified
    // for specific group and role
    acl = _groupRoleAcls.FirstOrDefault(x =>
    !x.ImageableId.HasValue &&
      x.ImageableType == objectType &&
      x.GroupId == groupId &&
      x.RoleId == roleId);

    if (acl != null)
      return (acl.Acl2 & requestedAcl) == requestedAcl;

    // test for object of any type specified
    // for specific group and role
    acl = _groupRoleAcls.FirstOrDefault(x =>
      !x.ImageableId.HasValue &&
      string.IsNullOrEmpty(x.ImageableType) &&
      x.GroupId == groupId &&
      x.RoleId == roleId);

    if (acl != null)
      return (acl.Acl2 & requestedAcl) == requestedAcl;

    // else return default acl
    acl = _groupRoleAcls.FirstOrDefault(x =>
      !x.GroupId.HasValue &&
      !x.RoleId.HasValue &&
      !x.ImageableId.HasValue &&
      string.IsNullOrEmpty(x.ImageableType));

    if (acl != null)
      return (acl.Acl2 & requestedAcl) == requestedAcl;

    return null;

  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToNodeAsync(ulong requestedAcl, uint mapNodeId)
  {
    bool result = false;

    var mapNodePhys = await MapNodesReaderWriter.Instance(GetLogger(), GetDbContext(), null)
      .GetNodeAsync(mapNodeId);

    if (mapNodePhys == null)
      throw new OLabObjectNotFoundException(Constants.ScopeLevelNode, mapNodeId);

    foreach (var userGroupRolePhys in _userGroupRoles)
    {
      var roleResult = await HasRequestedAccessAsync(
        userGroupRolePhys.GroupId,
        userGroupRolePhys.RoleId,
        Constants.ScopeLevelNode,
        mapNodePhys.Id,
        requestedAcl);

      if (roleResult.HasValue)
      {
        result = roleResult.Value;
        break;
      }
      else
        // test if no acl at node level, try the owning map
        result = await HasRequestedAccessToMapAsync(requestedAcl, mapNodePhys.MapId);
    }

    return result;
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToMapAsync(ulong requestedAcl, uint mapId)
  {
    bool result = false;

    var mapPhys = await MapsReaderWriter.Instance(GetLogger(), GetDbContext())
      .GetSingleWithGroupsAsync(mapId);

    if (mapPhys == null)
      throw new OLabObjectNotFoundException(Constants.ScopeLevelMap, mapId);

    foreach (var mapGroupPhys in mapPhys.MapGroups)
    {
      // see if user has any perms for the current map group
      if (!UserContext.GroupRoles.Any(x => x.GroupId == mapGroupPhys.GroupId))
        continue;

      foreach (var userGroupRolePhys in _userGroupRoles.Where(x => x.GroupId == mapGroupPhys.GroupId))
      {
        var roleResult = await HasRequestedAccessAsync(
          userGroupRolePhys.GroupId,
          userGroupRolePhys.RoleId,
          Constants.ScopeLevelMap,
          mapPhys.Id,
          requestedAcl);

        if (roleResult.HasValue)
        {
          result = roleResult.Value;
          break;
        }
      }

      if (result)
        break;
    }

    return result;
  }

  public async Task<bool> HasAccessAsync(
    ulong requestedAcl,
    string objectType,
    uint? objectId)
  {
    bool result = false;

    // test if user has access to map.
    if (objectType == Constants.ScopeLevelMap)
      result = await HasRequestedAccessToMapAsync(requestedAcl, objectId.Value);

    else if (objectType == Constants.ScopeLevelNode)
      result = await HasRequestedAccessToNodeAsync(requestedAcl, objectId.Value);

    return result;
  }

  public async Task<bool> HasAccessAsync(ulong requestedPerm, string operationType)
  {
    return await HasAccessAsync(requestedPerm, operationType, 0);
  }

  /// <summary>
  /// Test if user has access to application
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  /// <param name="refererValue">Request referer header value</param>
  /// <returns></returns>
  public async Task<bool> HasAccessToAppAsync(Users userPhys, string refererValue)
  {
    // load the user's acls
    ApplyUserContext(userPhys);

    var uri = new Uri(refererValue);

    // if no path, and referrer from locahost then this is probably local
    if (uri.PathAndQuery == "/" && uri.IdnHost == "localhost")
      return true;

    var appName = uri.PathAndQuery.Replace("/", "");
    GetLogger().LogInformation($"Testing referrer: '{uri.IdnHost}', '{appName}'");

    var appPhys = await GetDbContext().SystemApplications.FirstOrDefaultAsync(x => x.Name == appName);

    if (appPhys == null)
    {
      GetLogger().LogError($"Could not find application {refererValue} in database");
      return false;
    }

    foreach (var userGroupRolePhys in userPhys.UserGrouproles)
    {
      var accessResult = await HasRequestedAccessAsync(
        userGroupRolePhys.GroupId,
        userGroupRolePhys.RoleId,
        Constants.ScopeLevelApp,
        appPhys.Id,
        GrouproleAcls.ExecuteMask);

      if (accessResult.HasValue && accessResult.Value == true)
        return true;
    }

    return false;

  }
}