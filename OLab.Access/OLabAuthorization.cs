using Dawn;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
  public IList<GrouproleAcls> GroupRoleAcls { get; set; } = new List<GrouproleAcls>();
  public IList<UserGrouproles> UsersGroupRoles { get; set; } = new List<UserGrouproles>();
  protected IList<UserAcls> _userAcls = new List<UserAcls>();

  public Users OLabUser { get; set; }
  public IAuthenticatedContext AuthenticatedContext { get; set; }
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
    UsersGroupRoles = OLabUser.UserGrouproles.ToList();
    GroupRoleAcls = GetGroupRoleAclsAsync().GetAwaiter().GetResult();

    // JSON extraction scratch pad

    //var obj = MapsReaderWriter.Instance( _logger, _dbContext ).GetSingleWithGroupRolesAsync( 5 ).GetAwaiter().GetResult();
    //var obj = RoleReaderWriter.Instance( _logger, _dbContext ).GetPagedAsync(null, null).GetAwaiter().GetResult();
    //var obj = _dbContext.SystemApplications.ToList();
    //var obj = UserReaderWriter.Instance( _logger, _dbContext ).GetSingleAsync( "wirunc" ).GetAwaiter().GetResult();

    //var objs = new List<Maps>
    //{
    //  MapsReaderWriter.Instance( _logger, _dbContext ).GetSingleWithGroupRolesAsync( 5 ).GetAwaiter().GetResult(),
    //  MapsReaderWriter.Instance( _logger, _dbContext ).GetSingleWithGroupRolesAsync( 45 ).GetAwaiter().GetResult()
    //};

    //var obj = GroupRoleAclReaderWriter.Instance( _logger, _dbContext ).GetAsync().GetAwaiter().GetResult();
    //var json = StringUtils.TruncateToJsonObject( obj, 2 );
  }

  /// <summary>
  /// Add user context to Authorization and load group/role acls
  /// </summary>
  /// <param name="authenticatedContext">User context</param>
  public async Task ApplyUserContextAsync(IAuthenticatedContext authenticatedContext)
  {
    Guard.Argument( authenticatedContext ).NotNull( nameof( authenticatedContext ) );

    AuthenticatedContext = authenticatedContext;
    OLabUser = await _userReaderWriter.GetSingleAsync( AuthenticatedContext.UserId );

    // user might be external, so just create a virtual user
    if ( OLabUser == null )
      OLabUser = new Users { Id = AuthenticatedContext.UserId, Username = AuthenticatedContext.UserName };

    Issuer = AuthenticatedContext.Issuer;

    UsersGroupRoles = AuthenticatedContext.GroupRoles.ToList();
    GroupRoleAcls = await GetGroupRoleAclsAsync();
  }

  private async Task<IList<GrouproleAcls>> GetGroupRoleAclsAsync()
  {
    var aclsList = new List<GrouproleAcls>();

    // load all the user's group/roles acl records
    foreach ( var userGroups in UsersGroupRoles.Select( x => x.Group ).Distinct() )
    {
      var groupsPhys
        = await _groupRoleAclWriter.GetListAsync( userGroups.Id );
      aclsList.AddRange( groupsPhys );

      // add default no-group acls
      groupsPhys
        = (await _groupRoleAclWriter.GetRawAsync<GrouproleAcls>()).items.ToList();
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

    return UsersGroupRoles.Any( x => (x.GroupId == groupId) && (x.RoleId == superUserRolePhys.Id) );
  }

  private async Task<bool> HasRequestedAccessToMapAsync(
    ulong requestedAcl,
    uint mapId)
  {
    Maps phys = await MapsReaderWriter.Instance( _logger, GetDbContext() )
        .GetSingleWithGroupRolesAsync( mapId );
    return await HasRequestedAccessToMapAsync( requestedAcl, phys );
  }

  private async Task<bool> HasRequestedAccessToMapAsync(
    ulong requestedAcl,
    Maps phys)
  {
    foreach ( var userGroupRole in UsersGroupRoles )
    {
      // test if map is accessible purely based on group/role
      if ( !Maps.IsAccessible( phys, userGroupRole.GroupId, userGroupRole.RoleId ) )
        continue;

      var accessResult = await EvaluateAclHierarchyAsync(
        userGroupRole.GroupId,
        userGroupRole.RoleId,
        Constants.ScopeLevelMap,
        phys.Id,
        requestedAcl );

      if ( accessResult )
        return accessResult;
    }

    return false;
  }


  private async Task<bool> HasRequestedAccessToNodeAsync(
    ulong requestedAcl,
    uint nodeId)
  {
    MapNodes phys
      = await MapNodesReaderWriter.Instance( _logger, GetDbContext(), null ).GetNodeAsync( nodeId );
    return await HasRequestedAccessToNodeAsync( requestedAcl, phys );
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="nodeId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToNodeAsync(
    ulong requestedAcl,
    MapNodes phys)
  {
    bool hasAccess = true;

    var physMap = await MapsReaderWriter.Instance( GetLogger(), GetDbContext() ).GetSingleAsync( phys.MapId );
    if ( physMap == null )
      throw new OLabObjectNotFoundException( Constants.ScopeLevelMap, phys.MapId );

    var mapResult = await HasRequestedAccessToMapAsync( GrouproleAcls.ReadMask, phys.MapId );
    if ( !mapResult )
    {
      GetLogger().LogInformation( $"user has no access to mapId {phys.MapId} belonging to node {phys.Id}" );
      hasAccess = false;
    }

    // if have access to map, then test node
    if ( hasAccess )
    {
      hasAccess = false;

      foreach ( var userGroupRole in UsersGroupRoles )
      {
        // test if map belongs to one of the users groups
        if ( physMap.MapGrouproles.Select( x => x.GroupId ).Contains( userGroupRole.GroupId ) )
        {
          hasAccess = await EvaluateAclHierarchyAsync(
            userGroupRole.GroupId,
            userGroupRole.RoleId,
            Constants.ScopeLevelNode,
            phys.Id,
            requestedAcl );

          if ( hasAccess )
            break;
        }

      }
    }

    return hasAccess;
  }


  /// <summary>
  /// Test if user has access to application
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  /// <param name="refererValue">Request referer header value</param>
  /// <returns></returns>
  public async Task<bool> HasAccessToAppAsync(
    Users userPhys,
    string referrerUri)
  {
    // load the acls based on physical user's group/roles
    ApplyUserContext( userPhys );

    var applicationName = ExtractApplicationFromUri( referrerUri );
    var appPhys = await GetDbContext().SystemApplications.FirstOrDefaultAsync( x => x.Name == applicationName );
    if ( appPhys == null )
    {
      GetLogger().LogError( $"Could not find application '{applicationName}' in database" );
      return false;
    }
    else
      GetLogger().LogInformation( $"Found application '{applicationName}'" );

    foreach ( var physUserGroupRole in UsersGroupRoles )
    {
      var accessResult = await EvaluateAclHierarchyAsync(
        physUserGroupRole.GroupId,
        physUserGroupRole.RoleId,
        Constants.ScopeLevelApp,
        appPhys.Id,
        GrouproleAcls.ExecuteMask );

      if ( accessResult )
      {
        GetLogger().LogInformation( $"User '{userPhys.Username}' has application access under group role '{physUserGroupRole}'" );
        return true;
      }
    }

    GetLogger().LogError( $"user '{userPhys.Username}' does not have access to application '{applicationName}'" );
    return false;

  }

  /// <summary>
  /// Get applicablel ACL for group/role/object
  /// </summary>
  /// <param name="groupId"></param>
  /// <param name="roleId"></param>
  /// <param name="objectType"></param>
  /// <param name="objectId"></param>
  /// <returns>Applicable GroupRoleAcl record</returns>
  private GrouproleAcls GetAcl(
    uint? groupId,
    uint? roleId,
    string objectType,
    uint? objectId)
  {
    // groupId, roleId, objectType, objectId
    // #        #       #           #
    var acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: {objectId} = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // #        #       #           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: {objectType} id: null = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // #        #       -           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == roleId &&
      x.ImageableType == null &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: {roleId} typ: null id: null = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // #        -       #           #
    acl = GroupRoleAcls.FirstOrDefault( x =>
    x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: null typ: {objectType} id: {objectId} = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // -        #       #           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: null rol: {roleId} typ: {objectType} id: null = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // -        #       #           #
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == roleId &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: null rol: {roleId} typ: {objectType} id: {objectId} = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // #        -       #           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == groupId &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: {groupId} rol: null typ: {objectType} id: null = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // -        -       #           #
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      (x.ImageableId.HasValue && x.ImageableId.Value == objectId) );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: {objectType} id: {objectId} = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // -        -       #           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == objectType &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: {objectType} id: null = {acl.Acl2}" );
      return acl;
    }

    // groupId, roleId, objectType, objectId
    // -        -       -           -
    acl = GroupRoleAcls.FirstOrDefault( x =>
      x.GroupId == null &&
      x.RoleId == null &&
      x.ImageableType == null &&
      x.ImageableId == null );

    if ( acl != null )
    {
      GetLogger().LogInformation( $"    ACL: grp: null rol: null typ: null id: null = {acl.Acl2}" );
      return acl;
    }

    return null;
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
  private async Task<bool> EvaluateAclHierarchyAsync(
    uint? groupId,
    uint? roleId,
    string objectType,
    uint? objectId,
    ulong requestedAcl)
  {
    // group = any
    // role = superuser
    if ( groupId.HasValue && await IsGroupSuperUserAsync( groupId.Value ) )
      return true;

    if ( objectId == 0 )
      objectId = null;

    var acl = GetAcl( groupId, roleId, objectType, objectId );
    if ( acl != null )
    {
      var rc = (acl.Acl2 & requestedAcl) == requestedAcl;
      if ( !rc )
        GetLogger().LogError( $"no access" );
      return rc;
    }

    GetLogger().LogError( $"g: {groupId} r: {roleId} t: {objectType} i: {objectId} = {requestedAcl} no ACL applies" );

    return false;
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
    var corsParts = _configuration.GetAppSettings().Cors.Replace( " ", "" ).Split( ',' ).ToList();

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
    var groupRole = UsersGroupRoles.FirstOrDefault( x => roleIds.Contains( x.RoleId ) );
    return new MapGrouproles { MapId = map.Id, GroupId = groupRole.GroupId, RoleId = null };
  }

  /// <summary>
  /// Extracts the application name from the given request URI.
  /// </summary>
  /// <param name="requestUri">The request URI from which to extract the application name.</param>
  /// <returns>The extracted application name.</returns>
  public string ExtractApplicationFromUri(string requestUri)
  {
    var url = new Uri( requestUri );

    string path = string.Empty;
    if ( url.Segments.Count() > 1 )
      path = $"/{url.Segments[ 1 ].Trim( '/' )}";

    return $"{url.Authority}{path}";
  }


  /// <summary>
  /// Get high-level list of potential maps for user based on user group roles
  /// </summary>
  /// <param name="userGrouproles"></param>
  /// <returns>Distinct map list</returns>
  public async Task<IEnumerable<Maps>> GetMapSetAsync()
  {
    var mapReaderWrite = MapsReaderWriter.Instance( _logger, GetDbContext() );
    var maps = new List<Maps>();

    var potentialMaps = new List<Maps>();
    foreach ( var userGrouprole in UsersGroupRoles )
    {
      var mapsSubList = await mapReaderWrite.GetWithGroupRoleAsync(
        userGrouprole.GroupId,
        userGrouprole.RoleId );

      potentialMaps.AddRange( mapsSubList );
    }

    foreach ( var map in potentialMaps.DistinctBy( x => x.Id ) )
    {
      if ( await HasRequestedAccessToMapAsync( GrouproleAcls.ReadMask, map ) )
        maps.Add( map );
    }

    return maps.OrderBy( x => x.Name );
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="acl"></param>
  /// <param name="scopeLevelMap"></param>
  /// <param name="id"></param>
  /// <returns></returns>
  public async Task<bool> HasAccessAsync(
    ulong acl,
    string scopeLevelMap,
    uint id)
  {
    if ( await IsSystemSuperuserAsync() )
      return true;

    if ( scopeLevelMap == Constants.ScopeLevelMap )
      return await HasRequestedAccessToMapAsync( acl, id );
    if ( scopeLevelMap == Constants.ScopeLevelNode )
      return await HasRequestedAccessToNodeAsync( acl, id );

    return false;

  }

  /// <summary>
  /// When have object, test for user access
  /// </summary>
  /// <param name="acl">Requested acl</param>
  /// <param name="phys">Object to test</param>
  /// <returns>true/false</returns>
  public async Task<bool> HasAccessAsync(
    ulong acl,
    Maps phys)
  {
    if ( await IsSystemSuperuserAsync() )
      return true;

    return await HasRequestedAccessToMapAsync( acl, phys );
  }

  /// <summary>
  /// When have object, test for user access
  /// </summary>
  /// <param name="acl">Requested acl</param>
  /// <param name="phys">Object to test</param>
  /// <returns>true/false</returns>
  public async Task<bool> HasAccessAsync(
    ulong acl,
    MapNodes phys)
  {
    if ( await IsSystemSuperuserAsync() )
      return true;

    return await HasRequestedAccessToNodeAsync( acl, phys );
  }

  /// <summary>
  /// Test if have access to scoped object
  /// </summary>
  /// <param name="acl"></param>
  /// <param name="dto"></param>
  /// <returns>true/false</returns>
  public async Task<bool> HasAccessAsync(
    ulong acl,
    ScopedObjectDto dto)
  {
    if ( await IsSystemSuperuserAsync() )
      return true;

    // test if user has access to parent map.
    if ( dto.ImageableType == Constants.ScopeLevelMap )
    {
      var result = await HasRequestedAccessToMapAsync( acl, dto.ImageableId );

      if ( !result )
        return false;
    }

    // test if user has access to parent node.
    if ( dto.ImageableType == Constants.ScopeLevelNode )
    {
      var result = await HasRequestedAccessToNodeAsync( acl, dto.ImageableId );

      if ( !result )
        return false;
    }

    return true;
  }

  /// <summary>
  /// Get lsit of groups user is allowed to manage users for
  /// </summary>
  /// <returns>Group list</returns>
  public async Task<IList<Groups>> GetAuthorizedUserGroupsAsync()
  {
    var groups = ( await _groupReaderWriter.GetRawAsync<Groups>() ).items;

    // group = any
    // role = superuser
    if ( await IsSystemSuperuserAsync() )
      return groups.ToList();

    var allowedGroups = new List<Groups>(); 

    foreach ( var usersGroupRole in UsersGroupRoles )
    {
      var acl = GetAcl( usersGroupRole.GroupId, usersGroupRole.RoleId, "UserGroup", null );
      if ( acl != null )
      {
        var userHasUserGroupsAccess = ((acl.Acl2 & GrouproleAcls.ReadMask) == GrouproleAcls.ReadMask);
        if ( userHasUserGroupsAccess )
          allowedGroups.Add( groups.First( x => x.Id == usersGroupRole.GroupId ) );
      }
    }

    GetLogger().LogInformation( $"user can manage users for groups {string.Join(',', allowedGroups.Select( x => x.Name ))}" );

    return allowedGroups;
  }
}