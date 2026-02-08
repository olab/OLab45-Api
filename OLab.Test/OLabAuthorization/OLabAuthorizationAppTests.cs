using OLab.Api.Model;
using OLab.Test.Utils;

namespace OLab.Test.OLabAuthorization;

public class OLabAuthorizationAppTests : OLabAuthorizationTests
{

  [Fact]
  public async Task GuestUser_OneGroupRole_HasNoAccessToDesignerAsync()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task GuestUser_OneGroupRole_HasNoAccessToSmtAsync()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task GuestUser_OneGroupRole_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://player.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task LearnerUser_OneGroupRole_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.BuildQueryableListFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://player.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task LearnerUser_OneGroupRole_HasNoAccessToSmtAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.BuildQueryableListFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task LearnerUser_OneGroupRole_HasNoAccessToDesignerAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabGuest1GroupRole.json" ).First();
    var mapList = TestUtilities.BuildQueryableListFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.False( result );
  }

  [Fact]
  public async Task OLabSuperuserUser_HasAccessToSmtAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();
    var mapList = TestUtilities.BuildQueryableListFromJson<Maps>( "json\\Map5.json" );

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://smt.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task OLabSuperuserUser_HasAccessToDesignerAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();

    // Act
    var result = await _authorization.HasAccessToAppAsync( testUser, "http://designer.olab4.net" );

    // Assert
    Assert.True( result );
  }

  [Fact]
  public async Task OLabSuperuserUser_HasAccessToPlayerAsync()
  {
    var testUser = TestUtilities.BuildQueryableListFromJson<Users>( "json\\UserOLabSuperuser.json" ).First();
    var mapList = TestUtilities.BuildQueryableListFromJson<Maps>( "json\\Map5.json" );

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
    var expected = "example.com";

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
    var expected = "example.com";

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
    var expected = "example.com";

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
    var expected = "example.com";

    // Act
    var result = _authorization.ExtractApplicationFromUri( requestUri );

    // Assert
    Assert.Equal( expected, result );
  }
}
