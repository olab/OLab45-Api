using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    //var json = Access.AuthenticatedContext.TruncateToJsonObject( authenticatedContext, 4 );

    AuthenticatedContext = authenticatedContext;
    OLabUser = await _userReaderWriter.GetSingleAsync( AuthenticatedContext.UserId );

    // user might be external, so just create a virtual user
    if ( OLabUser == null )
      OLabUser = new Users { Id = AuthenticatedContext.UserId, Username = AuthenticatedContext.UserName };

    Issuer = AuthenticatedContext.Issuer;

    UsersGroupRoles = AuthenticatedContext.GroupRoles.ToList();
    GroupRoleAcls = await GetGroupRoleAclsAsync();

    //var obj = GroupRoleAclReaderWriter.Instance( _logger, _dbContext ).GetAsync().GetAwaiter().GetResult();
    //var json = StringUtils.TruncateToJsonObject( obj, 2 );
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
        = ( await _groupRoleAclWriter.GetRawAsync<GrouproleAcls>( null, null ) ).items.ToList();
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

      if ( !result.accessGranted )
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

    // group = olab
    // role = superuser
    if ( await IsSystemSuperuserAsync() )
      return true;

    // test if user has access to specified map.
    if ( objectType == Constants.ScopeLevelMap )
    {
      var mapResult = await HasRequestedAccessToMapAsync( requestedAcl, objectId.Value );
      result = mapResult.accessGranted;
    }

    // test if user has access to specified map.
    else if ( objectType == Constants.ScopeLevelNode )
      result = await HasRequestedAccessToNodeAsync( requestedAcl, objectId.Value );

    if ( !result )
      GetLogger().LogWarning( $"  user {AuthenticatedContext.Issuer}:{AuthenticatedContext.UserId} no access to {objectType} id {objectId.Value}" );

    return result;
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<(bool accessGranted, Maps physMap)> HasRequestedAccessToMapAsync(ulong requestedAcl, uint mapId)
  {
    Maps phys = await MapsReaderWriter.Instance( _logger, GetDbContext() )
        .GetSingleWithGroupRolesAsync( mapId );
    if ( phys == null )
      throw new OLabObjectNotFoundException( Constants.ScopeLevelMap, mapId );

    foreach ( var userGroupRole in UsersGroupRoles )
    {
      // test if map belongs to one of the users groups
      if ( phys.MapGrouproles.Select( x => x.GroupId ).Contains( userGroupRole.GroupId ) )
      {
        // test if user is superuser to a group map belongs to
        if ( await IsGroupSuperUserAsync( userGroupRole.GroupId ) )
          return (true, phys);

        var accessResult = await HasRequestedAccessAsync(
          userGroupRole,
          Constants.ScopeLevelMap,
          mapId,
          requestedAcl );

        if ( accessResult.HasValue && accessResult.Value == true )
          return (true, phys);
      }

    }

    return (false, phys);
  }

  /// <summary>
  /// Test if user has requested access to object
  /// </summary>
  /// <param name="requestedAcl"></param>
  /// <param name="mapId">Object id to search for</param>
  /// <returns>true/false</returns>
  private async Task<bool> HasRequestedAccessToNodeAsync(ulong requestedAcl, uint mapNodeId)
  {
    MapNodes phys = await MapNodesReaderWriter.Instance( _logger, GetDbContext(), null )
      .GetNodeAsync( mapNodeId );
    if ( phys == null )
      throw new OLabObjectNotFoundException( Constants.ScopeLevelNode, mapNodeId );

    var result = await HasRequestedAccessToMapAsync( GrouproleAcls.ExecuteMask, phys.MapId );
    if ( !result.accessGranted )
    {
      GetLogger().LogInformation( $"user has no access to mapId {phys.MapId} belonging to node {mapNodeId}" );
      return false;
    }

    foreach ( var userGroupRole in UsersGroupRoles )
    {
      // test if map belongs to one of the users groups
      if ( result.physMap.MapGrouproles.Select( x => x.GroupId ).Contains( userGroupRole.GroupId ) )
      {
        // test if user is superuser to a group map belongs to
        if ( await IsGroupSuperUserAsync( userGroupRole.GroupId ) )
          return true;

        var accessResult = await HasRequestedAccessAsync(
          userGroupRole,
          Constants.ScopeLevelNode,
          mapNodeId,
          requestedAcl );

        if ( accessResult.HasValue && accessResult.Value == true )
          return true;
      }

    }

    return false;
  }


  /// <summary>
  /// Test if user has access to application
  /// </summary>
  /// <param name="userPhys">User to evaluate</param>
  /// <param name="refererValue">Request referer header value</param>
  /// <returns></returns>
  public async Task<bool> HasAccessToAppAsync(Users userPhys, string referrerUri)
  {
    // load the acls based on physical user's group/roles
    ApplyUserContext( userPhys );
    GetLogger().LogInformation( $"Testing referrer: '{referrerUri}'" );

    var applicationName = ExtractApplicationFromUri( referrerUri );
    var appPhys = await GetDbContext().SystemApplications.FirstOrDefaultAsync( x => x.Name == applicationName );
    if ( appPhys == null )
    {
      GetLogger().LogError( $"Could not find application '{referrerUri}' in database" );
      return false;
    }

    foreach ( var physUserGroupRole in UsersGroupRoles )
    {
      var accessResult = await HasRequestedAccessAsync(
        physUserGroupRole,
        Constants.ScopeLevelApp,
        appPhys.Id,
        GrouproleAcls.ExecuteMask );

      if ( accessResult.HasValue && accessResult.Value == true )
        return true;

    }

    GetLogger().LogError( $"user '{userPhys.Username}' does not have access to application '{appPhys.Name}'" );
    return false;

  }

  private async Task<bool?> HasRequestedAccessAsync(
    UserGrouproles userGroupRole,
    string objectType,
    uint? objectId,
    ulong requestedAcl)
  {
    return await HasRequestedAccessAsync(
      userGroupRole.GroupId,
      userGroupRole.RoleId,
      objectType,
      objectId,
      requestedAcl );
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

    return $"{url.Host}{path}";
  }
}