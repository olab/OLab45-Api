using Dawn;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

/// <summary>
/// Provides authorization services for OLab, including user context application,
/// access checks, and superuser status verification.
/// </summary>
public class OLabAuthorization : IOLabAuthorization
{
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabConfiguration _configuration;
  private readonly GroupReaderWriter _groupReaderWriter;
  private readonly RoleReaderWriter _roleReaderWriter;
  private readonly UserReaderWriter _userReaderWriter;
  private readonly GroupRoleAclReaderWriter _groupRoleAclWriter;
  public IList<GrouproleAcls> GroupRoleAcls = new List<GrouproleAcls>();
  public IList<UserGrouproles> UserGroupRoles = new List<UserGrouproles>();
  protected IList<UserAcls> _userAcls = new List<UserAcls>();

  public Users OLabUser { get; set; }
  public IUserContext UserContext { get; set; }
  public string Issuer { get; set; }
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";

  public OLabDBContext GetDbContext() { return _dbContext; }

  protected IOLabLogger GetLogger() { return _logger; }

  public OLabAuthorization(
    IOLabLogger logger,
    OLabDBContext dbContext,
    IOLabConfiguration configuration
  )
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );
    Guard.Argument( dbContext ).NotNull( nameof( dbContext ) );
    Guard.Argument( configuration ).NotNull( nameof( configuration ) );

    _logger = logger;
    _dbContext = dbContext;
    _configuration = configuration;
    _groupReaderWriter = GroupReaderWriter.Instance( logger, dbContext );
    _roleReaderWriter = RoleReaderWriter.Instance( logger, dbContext );
    _userReaderWriter = UserReaderWriter.Instance( _logger, GetDbContext() );
    _groupRoleAclWriter = GroupRoleAclReaderWriter.Instance( _logger, GetDbContext() );
  }

  /// <summary>
  /// Add user Authorization and load group/role acls
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  public void ApplyUserContext(Users userPhys)
  {
    Guard.Argument( userPhys ).NotNull( nameof( userPhys ) );

    OLabUser = userPhys;
    Issuer = "olab";
    UserGroupRoles = OLabUser.UserGrouproles.ToList();
    GroupRoleAcls = GetGroupRoleAcls();

    //var obj = MapsReaderWriter.Instance( _logger, _dbContext ).GetSingleWithGroupRolesAsync( 5 ).GetAwaiter().GetResult();
    //var obj = RoleReaderWriter.Instance( _logger, _dbContext ).GetPagedAsync(null, null).GetAwaiter().GetResult();
    //var obj = _dbContext.SystemApplications.ToList();
    //var obj = UserReaderWriter.Instance( _logger, _dbContext ).GetSingleAsync("guest").GetAwaiter().GetResult();
    //var json = JsonConvert.SerializeObject( obj, new JsonSerializerSettings()
    //{
    //  ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    //  MaxDepth = 3
    //} );
  }

  /// <summary>
  /// Add user context to Authorization and load group/role acls
  /// </summary>
  /// <param name="userContext">User context</param>
  public async Task ApplyUserContextAsync(IUserContext userContext)
  {
    Guard.Argument( userContext ).NotNull( nameof( userContext ) );

    UserContext = userContext;
    OLabUser = await _userReaderWriter.GetSingleAsync( UserContext.UserId );

    // user might be external, so just create a virtual user
    if ( OLabUser == null )
      OLabUser = new Users { Id = UserContext.UserId, Username = UserContext.UserName };

    Issuer = UserContext.Issuer;

    UserGroupRoles = UserContext.GroupRoles.ToList();
    GroupRoleAcls = GetGroupRoleAcls();
  }

  private IList<GrouproleAcls> GetGroupRoleAcls()
  {
    var aclsList = new List<GrouproleAcls>();

    // load all the user's group/roles acl records
    foreach ( var userGroups in UserGroupRoles.Select( x => x.Group ).Distinct() )
    {
      var groupsPhys
        = _groupRoleAclWriter.FindByGroup( userGroups.Name );
      aclsList.AddRange( groupsPhys );

      // add default no-group acls
      groupsPhys
        = _groupRoleAclWriter.FindByGroup();
      aclsList.AddRange( groupsPhys );
    }

    return aclsList.Distinct().ToList();
  }

  /// <summary>
  /// Test if user is system superuser 
  /// </summary>
  /// <returns>true/false</returns>
  public async Task<bool> IsSystemSuperuserAsync()
  {
    return await IsGroupSuperUserAsync( Groups.OLabGroup );
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupName">Group name to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUserAsync(string groupName)
  {
    var groupPhys = await _groupReaderWriter.GetAsync( groupName );
    if ( groupPhys == null )
    {
      GetLogger().LogError( $"group '{groupName}' not defined." );
      return false;
    }

    return await IsGroupSuperUserAsync( groupPhys.Id );
  }

  /// <summary>
  /// Test if user is superuser in group
  /// </summary>
  /// <param name="groupId">Group id to check</param>
  /// <returns>true/false</returns>
  public async Task<bool> IsGroupSuperUserAsync(uint groupId)
  {
    var superUserRolePhys = await _roleReaderWriter.GetAsync( Roles.SuperUserRole );
    if ( superUserRolePhys == null )
    {
      GetLogger().LogError( $"system role {Roles.SuperUserRole} not defined." );
      return false;
    }

    return UserGroupRoles.Any( x => (x.GroupId == groupId) && (x.RoleId == superUserRolePhys.Id) );
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
    if ( dto.ImageableType == Constants.ScopeLevelMap )
    {
      var result = await HasRequestedAccessToMapAsync( requestedAcl, dto.ImageableId );

      if ( !result )
        return OLabUnauthorizedResult.Result();
    }


    // test if user has access to parent node.
    if ( dto.ImageableType == Constants.ScopeLevelNode )
    {
      var result = await HasRequestedAccessToNodeAsync( requestedAcl, dto.ImageableId );

      if ( !result )
        return OLabUnauthorizedResult.Result();
    }

    return new NoContentResult();
  }

  /// <summary>
  /// Checks if the user has access to a specific object type and ID with the requested permissions.
  /// </summary>
  /// <param name="requestedAcl">The requested access control list (ACL) permissions.</param>
  /// <param name="objectType">The type of the object to check access for.</param>
  /// <param name="objectId">The ID of the object to check access for.</param>
  /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the user has access.</returns>
  public async Task<bool> HasAccessAsync(
    ulong requestedAcl,
    string objectType,
    uint? objectId)
  {
    var result = false;

    // test if system superuser meaning unconditional access
    if ( await IsSystemSuperuserAsync() )
      return true;

    // test if user has access to specified map.
    if ( objectType == Constants.ScopeLevelMap )
      result = await HasRequestedAccessToMapAsync( requestedAcl, objectId.Value );

    if ( !result )
      GetLogger().LogWarning( $"  user {UserContext.Issuer}:{UserContext.UserId} no access to {objectType} id {objectId.Value}" );

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
    if ( mapId > 0 )
    {
      var mapPhys = await MapsReaderWriter.Instance( _logger, GetDbContext() )
        .GetSingleWithGroupRolesAsync( mapId );

      if ( mapPhys == null )
        throw new OLabObjectNotFoundException( Constants.ScopeLevelMap, mapId );
    }

    foreach ( var userGroupRole in UserGroupRoles )
    {
      var accessResult = await HasRequestedAccessAsync(
        userGroupRole.GroupId,
        userGroupRole.RoleId,
        Constants.ScopeLevelMap,
        mapId,
        requestedAcl );

      if ( accessResult.HasValue && accessResult.Value == true )
        return true;
    }

    return false;
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToNodeAsync(ulong requestedAcl, uint mapNodeId)
  {
    var mapNodePhys = await MapNodesReaderWriter.Instance( _logger, GetDbContext(), null )
      .GetNodeAsync( mapNodeId );

    if ( mapNodePhys == null )
      throw new OLabObjectNotFoundException( Constants.ScopeLevelNode, mapNodeId );

    // test base case of node not having any group/roles defined,
    // meaning check owning map for access
    if ( mapNodePhys.MapNodeGrouproles.Count == 0 )
      return await HasRequestedAccessToMapAsync( requestedAcl, mapNodePhys.MapId );
    else
    {
      foreach ( var nodeGroupRolePhys in mapNodePhys.MapNodeGrouproles )
      {
        // test if map has group and role and user has same
        if ( (nodeGroupRolePhys.GroupId != null) &&
            (nodeGroupRolePhys.RoleId != null) &&
            UserGroupRoles.Any( x => (x.GroupId == nodeGroupRolePhys.GroupId) && (x.RoleId == nodeGroupRolePhys.RoleId) ) )
          return true;

        // test if map has no group and role and user has same role
        if ( (nodeGroupRolePhys.GroupId == null) &&
            (nodeGroupRolePhys.RoleId != null) &&
            UserGroupRoles.Any( x => x.RoleId == nodeGroupRolePhys.RoleId ) )
          return true;

        // test if map has group and no role specified and
        // user belongs to any role in same group
        if ( (nodeGroupRolePhys.GroupId != null) &&
            (nodeGroupRolePhys.RoleId == null) &&
            UserGroupRoles.Any( x => (x.GroupId == nodeGroupRolePhys.GroupId) ) )
          return true;

        // test if map has no group and no role specified 
        // meaning unconditional 'accept'
        if ( (nodeGroupRolePhys.GroupId == null) &&
            (nodeGroupRolePhys.RoleId == null) )
          return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Checks if the user has the requested access to a specific object type.
  /// </summary>
  /// <param name="objectType">The type of the object to check access for.</param>
  /// <param name="requestedAcl">The requested access control list (ACL) permissions.</param>
  /// <returns>True if the user has the requested access; otherwise, false.</returns>
  private bool HasRequestedAccessToType(string objectType, ulong requestedAcl)
  {
    var acl = GroupRoleAcls.FirstOrDefault( x =>
      x.ImageableType == objectType &&
      x.ImageableId == 0 );

    if ( acl == null )
      return false;

    return (acl.Acl2 & requestedAcl) == requestedAcl;
  }


  /// <summary>
  /// Test if user has access to application
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  /// <param name="refererValue">Request referer header value</param>
  /// <returns></returns>
  public async Task<bool> HasAccessToAppAsync(Users userPhys, string appName)
  {
    // load the acls based on physical user's group/roles
    ApplyUserContext( userPhys );
    GetLogger().LogInformation( $"Testing referrer: '{appName}'" );

    var appPhys = await GetDbContext().SystemApplications.FirstOrDefaultAsync( x => x.Name == appName );
    if ( appPhys == null )
    {
      GetLogger().LogError( $"Could not find application '{appName}' in database" );
      return false;
    }

    foreach ( var userGroupRolePhys in UserGroupRoles )
    {
      var accessResult = await HasRequestedAccessAsync(
        userGroupRolePhys.GroupId,
        userGroupRolePhys.RoleId,
        Constants.ScopeLevelApp,
        appPhys.Id,
        GrouproleAcls.ExecuteMask );

      if ( accessResult.HasValue && accessResult.Value == true )
        return true;

    }

    GetLogger().LogError( $"user '{userPhys.Username}' does not have access to application '{appPhys.Name}'" );
    return false;

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
    if ( await IsSystemSuperuserAsync() )
      return true;

    // group = any
    // role = superuser
    if ( groupId.HasValue && await IsGroupSuperUserAsync( groupId.Value ) )
      return true;

    if ( objectId == 0 )
      objectId = null;

    GetLogger().LogInformation( $"Testing: g: {groupId} r: {roleId} t: {objectType} i: {objectId} = {requestedAcl}" );

    // groupId, roleId, objectType, objectId
    // #        #       #           #
    var acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: {objectId} = {rc}" );
      return rc;
    }

    // # # # -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: null = {rc}" );
      return rc;
    }

    // # # - -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == null &&
      x.ImageableId == null );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: null id: null = {rc}" );
      return rc;
    }

    // # - # #
    acl = GroupRoleAcls.FirstOrDefault( x =>
    x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: null typ: {objectType} id: {objectId} = {rc}" );
      return rc;
    }

    // - # # #
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: null rol: {roleId} typ: {objectType} id: {objectId} = {rc}" );
      return rc;
    }

    // # - # -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: null typ: {objectType} id: null = {rc}" );
      return rc;
    }

    // - - # #
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: {objectType} id: {objectId} = {rc}" );
      return rc;
    }

    // - - # -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: {objectType} id: null = {rc}" );
      return rc;
    }

    // - - - -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == null &&
      x.ImageableId == null );

    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: null id: null = {rc}" );
      return rc;
    }

    return null;

  }

  /// <summary>
  /// Extracts the application name from the given referer URL.
  /// </summary>
  /// <param name="refererValue">The referer URL from which to extract the application name.</param>
  /// <returns>The extracted application name. If the referer URL is from localhost or has no path, returns "localhost".</returns>
  public string ExtractApplication(string refererValue)
  {
    refererValue = refererValue.Trim( '/' );
    var uri = new Uri( refererValue );

    // if no path, and referrer from localhost then this is probably local
    var corsParts = _configuration.GetAppSettings().Cors.Replace(" ", "").Split( ',' ).ToList();

    if ( uri.PathAndQuery == "/" && _configuration.GetAppSettings().Cors.Contains( refererValue ) )
      return "localhost";

    var appName = uri.PathAndQuery.Trim( '/' ).Split( '/' ).First();
    if ( string.IsNullOrEmpty( appName ) )
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

    // look for first author role for user
    var rolePhys = await _roleReaderWriter.GetAsync( Roles.AuthorRole );
    if ( rolePhys == null )
      throw new Exception( $"missing {Roles.AuthorRole} role configuration" );
    roleIds.Add( rolePhys.Id );

    // look for first superuser role for user
    rolePhys = await _roleReaderWriter.GetAsync( Roles.SuperUserRole );
    if ( rolePhys == null )
      throw new Exception( $"missing {Roles.SuperUserRole} role configuration" );
    roleIds.Add( rolePhys.Id );

    // find first user group role that is a author then superuser
    var groupRole = UserGroupRoles.FirstOrDefault( x => roleIds.Contains( x.RoleId ) );
    return new MapGrouproles { MapId = map.Id, GroupId = groupRole.GroupId, RoleId = null };
  }

}