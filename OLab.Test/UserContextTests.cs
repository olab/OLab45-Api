using Moq;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System.Security.Claims;

namespace OLab.Test;

public class UserContextTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly TestUserContext _userContext;

  public UserContextTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _userContext = new TestUserContext( _mockLogger.Object, _mockDbContext.Object );
  }

  [Fact]
  public void Constructor_WithValidParameters_InitializesProperties()
  {
    Assert.NotNull( _userContext.GetLogger() );
    Assert.NotNull( _userContext.GetDbContext() );
  }

  [Fact]
  public void SetHeaders_WithValidHeaders_SetsHeaders()
  {
    var headers = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
    _userContext.SetHeaders( headers );

    Assert.Equal( 2, _userContext.Headers.Count );
    Assert.Equal( "value1", _userContext.Headers[ "key1" ] );
    Assert.Equal( "value2", _userContext.Headers[ "key2" ] );
  }

  [Fact]
  public void SetClaims_WithValidClaims_SetsClaims()
  {
    var claims = new Dictionary<string, string> { { "claim1", "value1" }, { "claim2", "value2" } };
    _userContext.SetClaims( claims );

    Assert.Equal( 2, _userContext.Claims.Count );
    Assert.Equal( "value1", _userContext.Claims[ "claim1" ] );
    Assert.Equal( "value2", _userContext.Claims[ "claim2" ] );
  }

  [Fact]
  public void GetClaim_WithExistingClaim_ReturnsClaimValue()
  {
    var claims = new Dictionary<string, string> { { "claim1", "value1" } };
    _userContext.SetClaims( claims );

    var result = _userContext.GetClaim( "claim1" );

    Assert.Equal( "value1", result );
  }

  [Fact]
  public void GetClaim_WithNonExistingClaim_ThrowsException()
  {
    Assert.Throws<Exception>( () => _userContext.GetClaim( "nonExistingClaim" ) );
  }

  [Fact]
  public void GetHeader_WithExistingHeader_ReturnsHeaderValue()
  {
    var headers = new Dictionary<string, string> { { "header1", "value1" } };
    _userContext.SetHeaders( headers );

    var result = _userContext.GetHeader( "header1" );

    Assert.Equal( "value1", result );
  }

  [Fact]
  public void GetHeader_WithNonExistingHeader_ThrowsException()
  {
    Assert.Throws<Exception>( () => _userContext.GetHeader( "nonExistingHeader" ) );
  }

  [Fact]
  public void LoadUserContext_WithValidHeadersAndClaims_LoadsContext()
  {
    var headers = new Dictionary<string, string> { { "olabsessionid", "session123" } };
    var claims = new Dictionary<string, string>
            {
                { ClaimTypes.Name, "testuser" },
                { "iss", "issuer" },
                { "id", "1" },
                { "app", "testapp" },
                { ClaimTypes.Role, "olabinternal"  }
            };

    _userContext.SetHeaders( headers );
    _userContext.SetClaims( claims );
    _userContext.LoadUserContext();

    Assert.Equal( "session123", _userContext.SessionId );
    Assert.Equal( "testuser", _userContext.UserName );
    Assert.Equal( "issuer", _userContext.Issuer );
    Assert.Equal( (uint)1, _userContext.UserId );
    Assert.Equal( "testapp", _userContext.AppName );
  }

  private class TestUserContext : UserContext
  {
    public TestUserContext(IOLabLogger logger, OLabDBContext dbContext) : base( logger, dbContext ) { }

    public new void SetHeaders(IDictionary<string, string> headers) => base.SetHeaders( headers );
    public new void SetClaims(IDictionary<string, string> claims) => base.SetClaims( claims );
    public new string GetClaim(string key, bool isRequired = true) => base.GetClaim( key, isRequired );
    public new string GetHeader(string key, bool isRequired = true) => base.GetHeader( key, isRequired );
    public new void LoadUserContext() => base.LoadUserContext();
    public new IOLabLogger GetLogger() => base.GetLogger();
    public new OLabDBContext GetDbContext() => base.GetDbContext();
  }
}
