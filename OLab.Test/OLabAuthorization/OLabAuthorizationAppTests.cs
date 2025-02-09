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

public class OLabAuthorizationAppTests : OLabAuthorizationTests
{

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
