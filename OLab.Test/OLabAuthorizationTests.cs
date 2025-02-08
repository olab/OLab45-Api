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
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;

namespace OLab.Test;

public class OLabAuthorizationTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<IOLabConfiguration> _mockConfiguration;
  private readonly OLabAuthorization _authorization;

  public OLabAuthorizationTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();

    TestUtilities.LoadGroupRoleAclFile( _mockDbContext, "json\\GroupRoleAcls.json" );
    TestUtilities.LoadGroupFile( _mockDbContext, "json\\Groups.json" );
    TestUtilities.LoadRoleFile( _mockDbContext, "json\\Roles.json" );
    TestUtilities.LoadSystemApplicationsFromJson( _mockDbContext, "json\\SystemApplications.json" );

    _authorization = new OLabAuthorization(
        _mockLogger.Object,
        _mockDbContext.Object,
        _mockConfiguration.Object
    );

  }

  [Fact]
  public void ApplyUserContext_WithValidUser_SetsProperties()
  {
    var testUser = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserAStevan.json" ).First();

    // Act
    _authorization.ApplyUserContext( testUser );

    // Assert
    Assert.Equal( testUser, _authorization.OLabUser );
    Assert.Equal( "olab", _authorization.Issuer );
    Assert.Equal( testUser.UserGrouproles.Count, _authorization.UserGroupRoles.Count );
    Assert.Equal( 9, _authorization.GroupRoleAcls.Count );
  }

  [Fact]
  public async Task ApplyAuth_WithValidUser_HasAccessToMapAsync()
  {
    var testUser = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserAStevan.json" ).First();
    TestUtilities.LoadRecordsFromJson<Maps>( "json\\Map5.json" );

    // Act
    _authorization.ApplyUserContext( testUser );
    var result = await _authorization.HasAccessAsync(
      IOLabAuthorization.AclBitMaskFull, 
      "Maps", 5 );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task ApplyAuth_WithValidUser_HasAccessToDesignerAsync()
  {
    var testUser = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserAStevan.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "designer" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task ApplyAuth_WithGuestUser_HasNoAccessToDesignerAsync()
  {
    var testUser = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserGuest.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "designer" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task ApplyAuth_WithGuestUser_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserGuest.json" ).First();
    var mapList = TestUtilities.LoadRecordsFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "player" );

    // Assert
    Assert.True( result );
  }
}
