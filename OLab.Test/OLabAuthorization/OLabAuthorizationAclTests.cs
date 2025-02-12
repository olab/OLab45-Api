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

namespace OLab.Test.OLabAuthorizationTests;

public class OLabAuthorizationAclTests : OLabAuthorizationTests
{
  [Fact]
  public async Task NoMapsGroupUser_ReadEmptyMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserTestGroup.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserTestGroupContext.json" ).First();

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
  public async Task NoMapsGroupUser_TwoGroupMaps_ReadEmptyMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserTestGroup.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserTestGroupContext.json" ).First();

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
  public async Task OLabSuperuserUser_EditMap()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabSuperuser.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskFull,
      Constants.ScopeLevelMap,
      1 );

    // Assert
    Assert.True( result1 );
  }

  [Fact]
  public async Task OLabAuthorUser_EditMap()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabAuthor.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabAuthorContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskFull,
      Constants.ScopeLevelMap,
      1 );

    // Assert
    Assert.True( result1 );
  }

  [Fact]
  public async Task OLabLearnerUser_NoEditMap()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabLearner.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabLearnerContext.json" ).First();

    var mapList = TestUtilities.MoqMapFromJsonFile( _mockDbContext, "json\\Maps1Group.json" );

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Act
    var result1 = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskFull,
      Constants.ScopeLevelMap,
      1 );

    // Assert
    Assert.False( result1 );
  }

  [Fact]
  public async Task OLabSuperuserUser_TwoGroupMaps_HasCompleteMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabSuperuser.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

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
  public async Task ExternalUser_OneGroupRole_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal1GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserExternal1GroupRoleContext.json" ).First();

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
  public async Task ExternalUser_OneGroupRole_TwoGroupMaps_ReadilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal1GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserExternal1GroupRoleContext.json" ).First();

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
  public async Task ExternalUser_TwoGroupRole_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal2GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserExternal2GroupRoleContext.json" ).First();

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
  public async Task ExternalUser_TwoGroupRole_TwoGroupMaps_ReadFilteredMapList()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserExternal2GroupRole.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserExternal2GroupRoleContext.json" ).First();

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
}
