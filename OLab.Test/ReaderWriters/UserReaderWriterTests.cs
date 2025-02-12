using Moq;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using OLab.Test.Utils;
using DocumentFormat.OpenXml.Spreadsheet;
using Users = OLab.Api.Model.Users;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Moq.EntityFrameworkCore;

namespace OLab.Test.ReaderWriters;

public class UserReaderWriterTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly UserReaderWriter _userReaderWriter;

  public UserReaderWriterTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _userReaderWriter = new UserReaderWriter( _mockLogger.Object, _mockDbContext.Object );
  }

  [Fact]
  public async Task GetSingleAsync_WithValidName_ReturnsUser()
  {
    var user = new Users { Username = "testuser" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( x => x.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetSingleAsync( "testuser" );

    Assert.NotNull( result );
    Assert.Equal( "testuser", result.Username );
  }

  [Fact]
  public async Task GetSingleAsync_WithInvalidName_ReturnsNull()
  {
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users>() );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetSingleAsync( "invaliduser" );

    Assert.Null( result );
  }

  [Fact]
  public async Task GetSingleAsync_WithValidId_ReturnsUser()
  {
    var user = new Users { Id = 1 };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetSingleAsync( 1 );

    Assert.NotNull( result );
    Assert.Equal( 1u, result.Id );
  }

  [Fact]
  public async Task GetSingleAsync_WithInvalidId_ReturnsNull()
  {
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users>() );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetSingleAsync( 999 );

    Assert.Null( result );
  }

  [Fact]
  public async Task GetNameLikeAsync_WithMatchingName_ReturnsUsers()
  {
    var user = new Users { Username = "testuser", Nickname = "test nickname" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetNameLikeAsync( "test" );

    Assert.NotEmpty( result );
    Assert.Contains( result, u => u.Username == "testuser" );
  }

  [Fact]
  public async Task GetAsync_ReturnsAllUsers()
  {
    var user = new Users { Username = "testuser" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.GetAsync();

    Assert.NotEmpty( result );
    Assert.Contains( result, u => u.Username == "testuser" );
  }

  [Fact]
  public async Task CreateAsync_WithNewUser_CreatesUser()
  {
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users>() );
    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var user = new Users { Username = "newuser" };
    var result = await _userReaderWriter.CreateAsync( user );

    Assert.NotNull( result );
    _mockDbContext.Verify( db => db.Users.Add( user ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChanges(), Times.Once );
  }

  [Fact]
  public async Task CreateAsync_WithExistingUser_ReturnsNull()
  {
    var user = new Users { Username = "existinguser" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var newUser = new Users { Username = "existinguser" };
    var result = await _userReaderWriter.CreateAsync( newUser );

    Assert.Null( result );
    _mockDbContext.Verify( db => db.Users.Add( It.IsAny<Users>() ), Times.Never );
    _mockDbContext.Verify( db => db.SaveChanges(), Times.Never );
  }

  [Fact]
  public async Task DeleteAsync_WithValidId_DeletesUser()
  {
    var user = new Users { Id = 1, Username = "testuser" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.DeleteAsync( 1 );

    Assert.True( result );
    _mockDbContext.Verify( db => db.Users.Remove( user ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ), Times.Once );
  }

  [Fact]
  public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
  {
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users>() );
    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.DeleteAsync( 999 );

    Assert.False( result );
    _mockDbContext.Verify( db => db.Users.Remove( It.IsAny<Users>() ), Times.Never );
    _mockDbContext.Verify( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ), Times.Never );
  }

  [Fact]
  public async Task UpdateAsync_WithValidUser_UpdatesUser()
  {
    var user = new Users { Id = 1, Username = "testuser" };
    var users = TestUtilities.BuildQueryableListFromList<Users>( new List<Users> { user } );

    _mockDbContext.Setup( db => db.Users ).ReturnsDbSet( users );

    var result = await _userReaderWriter.UpdateAsync( user );

    Assert.NotNull( result );
    _mockDbContext.Verify( db => db.Users.Update( user ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ), Times.Once );

  }
}
