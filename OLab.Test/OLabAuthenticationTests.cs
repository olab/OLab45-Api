using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OLab.Api.Utils;
using OLab.Api.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using DocumentFormat.OpenXml.Spreadsheet;
using Users = OLab.Api.Model.Users;
using Moq.EntityFrameworkCore;

namespace OLab.Test;

public class OLabAuthenticationTests
{
  private readonly Mock<IOLabConfiguration> _mockConfig;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly OLabAuthentication _auth;

  public OLabAuthenticationTests()
  {
    _mockConfig = new Mock<IOLabConfiguration>();
    _mockDbContext = new Mock<OLabDBContext>();
    _mockLogger = new Mock<IOLabLogger>();

    _mockConfig.Setup( c => c.GetAppSettings() ).Returns( new AppSettings
    {
      Secret = "supersecretkeythatneedstobeatleast40characterslong",
      Issuer = "issuer",
      Audience = "audience",
      TokenExpiryMinutes = 120
    } );

    _auth = new OLabAuthentication( _mockLogger.Object, _mockConfig.Object, _mockDbContext.Object );
  }

  [Fact]
  public void ExtractAccessToken_ShouldReturnToken_WhenTokenIsInHeader()
  {
    var headers = new Dictionary<string, string> { { "Authorization", "Bearer testtoken" } };
    Assert.Throws<OLabUnauthorizedException>( () => _auth.ExtractAccessToken( headers ) );
  }

  [Fact]
  public void ExtractAccessToken_ShouldThrowException_WhenNoTokenProvided()
  {
    var headers = new Dictionary<string, string>();
    Assert.Throws<OLabUnauthorizedException>( () => _auth.ExtractAccessToken( headers ) );
  }

  [Fact]
  public void ValidateToken_ShouldThrowException_WhenTokenIsInvalid()
  {
    var invalidToken = "invalidtoken";
    Assert.Throws<SecurityTokenMalformedException>( () => _auth.ValidateToken( invalidToken ) );
  }

  [Fact]
  public void GenerateJwtToken_ShouldReturnAuthenticateResponse_WhenUserIsValid()
  {
    var user = new Users
    {
      Username = "testuser",
      Nickname = "Test User",
      Id = 1,
    };

    var response = _auth.GenerateJwtToken( user, "referrer" );
    Assert.NotNull( response );
    Assert.Equal( "testuser", response.UserName );
  }

  [Fact]
  public async void Authenticate_ShouldReturnUser_WhenCredentialsAreValid()
  {
    var loginRequest = new LoginRequest
    {
      Username = "wirunc",
      Password = "wirunc"
    };

    var users = new List<Users>
    {
      new Users
      {
        Username = "wirunc",
        Password = "7d7173fac102fcc5123f20a5a330477809504f48",
        Salt = "1baf4tk6q8w95zoa5v26xs7g0dli3fha5lddutayjshu89qaoctwfnbltleubz23"
      }
    }.AsQueryable();

    var mockSet = new Mock<DbSet<Users>>();
    mockSet.As<IQueryable<Users>>().Setup( m => m.Provider ).Returns( users.Provider );
    mockSet.As<IQueryable<Users>>().Setup( m => m.Expression ).Returns( users.Expression );
    mockSet.As<IQueryable<Users>>().Setup( m => m.ElementType ).Returns( users.ElementType );
    mockSet.As<IQueryable<Users>>().Setup( m => m.GetEnumerator() ).Returns( users.GetEnumerator() );

    // https://stackoverflow.com/questions/51023223/the-provider-for-the-source-iqueryable-doesnt-implement-iasyncqueryprovider
    _mockDbContext.Setup( x => x.Users ).ReturnsDbSet( mockSet.Object );

    var result = await _auth.AuthenticateAsync( loginRequest );
    Assert.NotNull( result );
    Assert.Equal( loginRequest.Username, result.Username );
  }

  [Fact]
  public async void Authenticate_ShouldReturnNull_WhenCredentialsAreInvalid()
  {
    var loginRequest = new LoginRequest
    {
      Username = "wirunc",
      Password = "test"
    };

    var users = new List<Users>
    {
      new Users
      {
        Username = "wirunc",
        Password = "7d7173fac102fcc5123f20a5a330477809504f48",
        Salt = "1baf4tk6q8w95zoa5v26xs7g0dli3fha5lddutayjshu89qaoctwfnbltleubz23"
      }
    }.AsQueryable();

    var mockSet = new Mock<DbSet<Users>>();
    mockSet.As<IQueryable<Users>>().Setup( m => m.Provider ).Returns( users.Provider );
    mockSet.As<IQueryable<Users>>().Setup( m => m.Expression ).Returns( users.Expression );
    mockSet.As<IQueryable<Users>>().Setup( m => m.ElementType ).Returns( users.ElementType );
    mockSet.As<IQueryable<Users>>().Setup( m => m.GetEnumerator() ).Returns( users.GetEnumerator() );

    // https://stackoverflow.com/questions/51023223/the-provider-for-the-source-iqueryable-doesnt-implement-iasyncqueryprovider
    _mockDbContext.Setup( x => x.Users ).ReturnsDbSet( mockSet.Object );

    var result = await _auth.AuthenticateAsync( loginRequest );
    Assert.Null( result );
  }
  [Fact]
  public void UpdatePassword_ShouldReturnTrue_WhenPasswordIsUpdated()
  {
    var user = new Users
    {
      Username = "testuser",
      Password = "oldpasswordhash",
      Salt = "oldsalt"
    };

    var newPassword = "newpassword";

    var result = _auth.UpdatePassword( newPassword, user );

    Assert.True( result );
    Assert.NotEqual( "oldpasswordhash", user.Password );
    Assert.NotEqual( "oldsalt", user.Salt );
  }

  [Fact]
  public void UpdatePassword_ShouldReturnFalse_WhenNewPasswordIsEmpty()
  {
    var user = new Users
    {
      Username = "testuser",
      Password = "oldpasswordhash",
      Salt = "oldsalt"
    };

    var newPassword = "";

    var result = _auth.UpdatePassword( newPassword, user );

    Assert.False( result );
    Assert.Equal( "oldpasswordhash", user.Password );
    Assert.Equal( "oldsalt", user.Salt );
  }

  [Fact]
  public void UpdatePassword_ShouldReturnFalse_WhenUserIsNull()
  {
    Users? user = null;
    var newPassword = "newpassword";

    Assert.Throws<ArgumentNullException>( () => _auth.UpdatePassword( newPassword, user ) );
  }

  [Fact]
  public void UpdatePassword_ShouldThrowArgumentNullException_WhenUserIsNull()
  {
    Users? user = null;
    var newPassword = "newpassword";

    Assert.Throws<ArgumentNullException>( () => _auth.UpdatePassword( newPassword, user ) );
  }
}

