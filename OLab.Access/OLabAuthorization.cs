using Dawn;
using Microsoft.AspNetCore.Mvc;
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

namespace OLab.Access;

public class OLabAuthorization : IOLabAuthorization
{
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabConfiguration _configuration;
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
    OLabDBContext dbContext,
    IOLabConfiguration configuration
  )
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(configuration).NotNull(nameof(configuration));

    _logger = logger;
    _dbContext = dbContext;
    _configuration = configuration;
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
  }

  /// <summary>
  /// Test if user is system superuser 
  /// </summary>
  /// <returns>true/false</returns>
  public async Task<bool> IsSystemSuperuserAsync()
  {
    return await IsGroupSuperUserAsync(Groups.OLabGroup);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupName">Group name to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUserAsync(string groupName)
  {
    var groupPhys = await _groupReaderWriter.GetAsync(groupName);
    if (groupPhys == null)
    {
      GetLogger().LogError($"group '{groupName}' not defined.");
      return false;
    }

    return await IsGroupSuperUserAsync(groupPhys.Id);
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupId">Group id to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUserAsync(uint groupId)
  {
    var superUserRolePhys = await _roleReaderWriter.GetAsync(Roles.SuperUserRole);
    if (superUserRolePhys == null)
    {
      GetLogger().LogError($"system role {Roles.SuperUserRole} not defined.");
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
      var result = await HasRequestedAccessToMapAsync(requestedAcl, dto.ImageableId);

      if (!result)
        return OLabUnauthorizedResult.Result();
    }


    // test if user has access to parent node.
    if (dto.ImageableType == Constants.ScopeLevelNode)
    {
      var result = await HasRequestedAccessToNodeAsync(requestedAcl, dto.ImageableId);

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
    // group = olab
    // role = superuser
    if (await IsSystemSuperuserAsync())
      return true;

    GetLogger().LogInformation($"Testing: g: {groupId} r: {roleId} t: {objectType} i: {objectId} = {requestedAcl}");

    // # # # #
    var acl = _groupRoleAcls.FirstOrDefault(x =>
    x.GroupId == groupId &&
    x.RoleId == roleId &&
    x.ImageableType == objectType &&
    (x.ImageableId.HasValue && x.ImageableId.Value == objectId));

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: {objectId} = {rc}");
      return rc;
    }

    // # # # -
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      x.ImageableId == null);

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: null = {rc}");
      return rc;
    }

    // # # - -
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == null &&
      x.ImageableId == null);

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: {groupId} rol: {roleId} typ: null id: null = {rc}");
      return rc;
    }

    // # - # #
    acl = _groupRoleAcls.FirstOrDefault(x =>
    x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId));

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: {groupId} rol: null typ: {objectType} id: {objectId} = {rc}");
      return rc;
    }

    // - # # #
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == null &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId));

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: null rol: {roleId} typ: {objectType} id: {objectId} = {rc}");
      return rc;
    }

    // # - # -
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null);

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: {groupId} rol: null typ: {objectType} id: null = {rc}");
      return rc;
    }

    // - - # #
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId));

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: null rol: null typ: {objectType} id: {objectId} = {rc}");
      return rc;
    }

    // - - # -
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null);

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: null rol: null typ: {objectType} id: null = {rc}");
      return rc;
    }

    // - - - -
    acl = _groupRoleAcls.FirstOrDefault(x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == null &&
      x.ImageableId == null);

    if (acl != null)
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation($"    ACL: grp: null rol: null typ: null id: null = {rc}");
      return rc;
    }

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
    var mapNodePhys = await MapNodesReaderWriter.Instance(GetLogger(), GetDbContext(), null)
      .GetNodeAsync(mapNodeId);

    if (mapNodePhys == null)
      throw new OLabObjectNotFoundException(Constants.ScopeLevelNode, mapNodeId);

    // test base case of node not having any group/roles defined,
    // meaning check owning map for access
    if (mapNodePhys.MapNodeGrouproles.Count == 0)
      return await HasRequestedAccessToMapAsync(requestedAcl, mapNodePhys.MapId);
    else
    {
      foreach (var nodeGroupRolePhys in mapNodePhys.MapNodeGrouproles)
      {
        // test if map has group and role and user has same
        if ((nodeGroupRolePhys.GroupId != null) &&
            (nodeGroupRolePhys.RoleId != null) &&
            UserContext.GroupRoles.Any(x => (x.GroupId == nodeGroupRolePhys.GroupId) && (x.RoleId == nodeGroupRolePhys.RoleId)))
          return true;

        // test if map has no group and role and user has same role
        if ((nodeGroupRolePhys.GroupId == null) &&
            (nodeGroupRolePhys.RoleId != null) &&
            UserContext.GroupRoles.Any(x => x.RoleId == nodeGroupRolePhys.RoleId))
          return true;

        // test if map has group and no role specified and
        // user belongs to any role in same group
        if ((nodeGroupRolePhys.GroupId != null) &&
            (nodeGroupRolePhys.RoleId == null) &&
            UserContext.GroupRoles.Any(x => (x.GroupId == nodeGroupRolePhys.GroupId)))
          return true;

        // test if map has no group and no role specified 
        // meaning unconditional 'accept'
        if ((nodeGroupRolePhys.GroupId == null) &&
            (nodeGroupRolePhys.RoleId == null))
          return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToMapAsync(ulong requestedAcl, uint mapId)
  {
    var mapPhys = await MapsReaderWriter.Instance(GetLogger(), GetDbContext())
      .GetSingleWithGroupRolesAsync(mapId);

    if (mapPhys == null)
      throw new OLabObjectNotFoundException(Constants.ScopeLevelMap, mapId);

    // test base case of map not having any group/roles defined,
    // meaning unconditional 'accept'
    if (mapPhys.MapGrouproles.Count == 0)
      return true;

    // loop thru map group roles and see if user has access based
    // on USER's group roles
    foreach (var mapGroupRolePhys in mapPhys.MapGrouproles)
    {
      // test if user is a superuser for the group that the map is in
      if (mapGroupRolePhys.GroupId.HasValue &&
          await IsGroupSuperUserAsync(mapGroupRolePhys.GroupId.Value))
        return true;

      // test if map has group and role and user has same
      if ((mapGroupRolePhys.GroupId != null) &&
          (mapGroupRolePhys.RoleId != null) &&
          UserContext.GroupRoles.Any(x =>
            (x.GroupId == mapGroupRolePhys.GroupId) &&
            (x.RoleId == mapGroupRolePhys.RoleId)))
        return true;

      // test if map has no group and has role and user has same role
      if ((mapGroupRolePhys.GroupId == null) &&
          (mapGroupRolePhys.RoleId != null) &&
          UserContext.GroupRoles.Any(x => x.RoleId == mapGroupRolePhys.RoleId))
        return true;

      // test if map has group and no role specified and
      // user belongs to any role in same group
      if ((mapGroupRolePhys.GroupId != null) &&
          (mapGroupRolePhys.RoleId == null) &&
          UserContext.GroupRoles.Any(x => (x.GroupId == mapGroupRolePhys.GroupId)))
        return true;

      // test if map has no group and no role specified 
      // meaning unconditional 'accept'
      if ((mapGroupRolePhys.GroupId == null) &&
          (mapGroupRolePhys.RoleId == null))
        return true;
    }

    return false;
  }

  public async Task<bool> HasAccessAsync(
    ulong requestedAcl,
    string objectType,
    uint? objectId)
  {
    var result = false;

    // test if system superuser meaning unconditional access
    if (await IsSystemSuperuserAsync())
      return true;

    // test if user has access to map.
    if (objectType == Constants.ScopeLevelMap)
      result = await HasRequestedAccessToMapAsync(requestedAcl, objectId.Value);

    // test if user has access to node
    else if (objectType == Constants.ScopeLevelNode)
      result = await HasRequestedAccessToNodeAsync(requestedAcl, objectId.Value);

    if (!result)
      GetLogger().LogWarning($"  user {UserContext.Issuer}:{UserContext.UserId} no access to {objectType} id {objectId.Value}");

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
  public async Task<bool> HasAccessToAppAsync(Users userPhys, string appName)
  {
    // load the user's acls
    ApplyUserContext(userPhys);
    GetLogger().LogInformation($"Testing referrer: '{appName}'");

    var appPhys = await GetDbContext().SystemApplications.FirstOrDefaultAsync(x => x.Name == appName);
    if (appPhys == null)
    {
      GetLogger().LogError($"Could not find application '{appName}' in database");
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

    GetLogger().LogError($"user '{userPhys.Username}' does not have access to application '{appPhys.Name}'");
    return false;

  }

  public string ExtractApplication(string refererValue)
  {
    refererValue = refererValue.Trim('/');
    var uri = new Uri(refererValue);

    // if no path, and referrer from localhost then this is probably local
    if (uri.PathAndQuery == "/" && _configuration.GetAppSettings().Cors.Contains(refererValue))
      return "localhost";

    var appName = uri.PathAndQuery.Trim('/').Split('/').First();
    if ( string.IsNullOrEmpty(appName ) )
      return "localhost";

    return appName;
  }

  /// <summary>
  /// Get default group/role for map created by user
  /// </summary>
  /// <returns>MapGrouproles record</returns>
  /// <exception cref="Exception">If missing configuration roles</exception>
  public async Task<MapGrouproles> GetMapCreationGroupRoleAsync(Maps map)
  {
    var roleIds = new List<uint>();
    var userGroupRoles = UserContext.GroupRoles;

    // look for first author role for user
    var rolePhys = await _roleReaderWriter.GetAsync(Roles.AuthorRole);
    if (rolePhys == null)
      throw new Exception($"missing {Roles.AuthorRole} role configuration");
    roleIds.Add(rolePhys.Id);

    // look for first superuser role for user
    rolePhys = await _roleReaderWriter.GetAsync(Roles.SuperUserRole);
    if (rolePhys == null)
      throw new Exception($"missing {Roles.SuperUserRole} role configuration");
    roleIds.Add(rolePhys.Id);

    // find first user group role that is a author then superuser
    var groupRole = userGroupRoles.FirstOrDefault(x => roleIds.Contains(x.RoleId));
    return new MapGrouproles { MapId = map.Id, GroupId = groupRole.GroupId, RoleId = null };
  }

}