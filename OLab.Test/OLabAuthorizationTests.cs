using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using NuGet.Packaging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using OLab.Test.Utils;
using Xunit.Abstractions;

namespace OLab.Test;

public class OLabAuthorizationTests
{
  private readonly IOLabLogger _logger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<IOLabConfiguration> _mockConfiguration;
  private readonly OLabAuthorization _authorization;
  private readonly IAuthenticatedContext _authenticatedContext;


  public OLabAuthorizationTests()
  {
    _logger = new Mock<IOLabLogger>().Object;

    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();

    TestUtilities.MoqGroupRoleAclFromJsonFile( _mockDbContext, "json\\GroupRoleAcls.json" );
    TestUtilities.MoqGroupsFromJsonFile( _mockDbContext, "json\\Groups.json" );
    TestUtilities.MoqRoleFromJsonFile( _mockDbContext, "json\\Roles.json" );
    TestUtilities.MoqSystemApplicationsFromJsonFile( _mockDbContext, "json\\SystemApplications.json" );

    _authorization = new OLabAuthorization(
        _logger,
        _mockDbContext.Object,
        _mockConfiguration.Object
    );

    _authenticatedContext 
      = new MoqAuthenticatedContext( _logger, _mockDbContext.Object );
  }

  [Fact]
  public async Task ApplyUserContext_WithValidUser_SetsProperties()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabSuperuser.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Assert
    Assert.Equal( testUser.Id, _authorization.OLabUser.Id );
    Assert.Equal( testUser.Username, _authorization.OLabUser.Username );
    Assert.Equal( "olab", _authorization.Issuer );
    Assert.Equal( testUser.UserGrouproles.Count, _authorization.UsersGroupRoles.Count );
    Assert.True( _authorization.GroupRoleAcls.Count > 0 );
  }

  [Fact]
  public async Task App_GuestUser_OneGroupRole_HasNoAccessToDesignerAsync()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task App_GuestUser_OneGroupRole_HasNoAccessToSmtAsync()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task App_GuestUser_OneGroupRole_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://player.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task Access_NoMapsGroupUser_ReadEmptyMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserTestGroup.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserTestGroupContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.False( result2 );
    Assert.False( result3 );
  }

  [Fact]
  public async Task Access_NoMapsGroupUser_TwoGroupMaps_ReadEmptyMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserTestGroup.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserTestGroupContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps2Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.False( result2 );
    Assert.False( result3 );
  }

  [Fact]
  public async Task Access_OLabSuperuserUser_HasCompleteMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabSuperuser.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.True( result1 );
    Assert.True( result2 );
    Assert.True( result3 );
  }

  [Fact]
  public async Task Access_OLabSuperuserUser_TwoGroupMaps_HasCompleteMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabSuperuser.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps2Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.True( result1 );
    Assert.True( result2 );
    Assert.True( result3 );
  }

  [Fact]
  public async Task Access_ExternalUser_OneGroupRole_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal1GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserExternal1GroupRoleContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync( 
      IOLabAuthorization.AclBitMaskRead, 
      Constants.ScopeLevelMap, 
      1 );

    var result2 = await _authorization.HasAccessAsync( 
      IOLabAuthorization.AclBitMaskRead, 
      Constants.ScopeLevelMap, 
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.True( result2 );
    Assert.False( result3 );
  }

  [Fact]
  public async Task Access_ExternalUser_OneGroupRole_TwoGroupMaps_ReadilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal1GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserExternal1GroupRoleContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps2Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.True( result2 );
    Assert.False( result3 );
  }

  [Fact]
  public async Task Access_ExternalUser_TwoGroupRole_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal2GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserExternal2GroupRoleContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.True( result2 );
    Assert.True( result3 );
  }

  [Fact]
  public async Task Access_ExternalUser_TwoGroupRole_TwoGroupMaps_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal2GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.LoadObjectFromJson<MoqAuthenticatedContext>( "json\\UserExternal2GroupRoleContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps2Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      1 );

    var result2 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      2 );

    var result3 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskRead,
      Constants.ScopeLevelMap,
      3 );

    // Assert
    Assert.False( result1 );
    Assert.True( result2 );
    Assert.True( result3 );
  }

  [Fact]
  public async Task App_LearnerUser_OneGroupRole_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.LoadObjectFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://player.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task App_LearnerUser_OneGroupRole_HasNoAccessToSmtAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.LoadObjectFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task App_LearnerUser_OneGroupRole_HasNoAccessToDesignerAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.LoadObjectFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task App_OLabSuperuserUser_HasAccessToSmtAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();
    var mapList = TestUtilities.LoadObjectFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task App_OLabSuperuserUser_HasAccessToDesignerAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task App_OLabSuperuserUser_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.LoadObjectFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();
    var mapList = TestUtilities.LoadObjectFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://player.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public void AppUri_WithValidUri_ReturnsCorrectApplicationKey()
  {
    // Arrange
    var requestUri = "https://example.com/app/path";
    var expected = "example.com/app";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }

  [Fact]
  public void AppUri_WithUriHavingNoPath_ReturnsHostOnly()
  {
    // Arrange
    var requestUri = "https://example.com/";
    var expected = "example.com";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }

  [Fact]
  public void AppUri_WithUriHavingMultipleSegments_ReturnsFirstSegment()
  {
    // Arrange
    var requestUri = "https://example.com/app/extra/path";
    var expected = "example.com/app";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }

  [Fact]
  public void AppUri_WithUriHavingNoSegments_ReturnsHostOnly()
  {
    // Arrange
    var requestUri = "https://example.com";
    var expected = "example.com";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }

  [Fact]
  public void AppUri_WithUriHavingSegments_ReturnsHostAndFirstPart()
  {
    // Arrange
    var requestUri = "https://example.com/path1/path2";
    var expected = "example.com/path1";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }

  [Fact]
  public void AppUri_WithUriHavingSegmentsAndQuery_ReturnsHostAndFirstPart()
  {
    // Arrange
    var requestUri = "https://example.com/path1/path2?query=1";
    var expected = "example.com/path1";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }
}
