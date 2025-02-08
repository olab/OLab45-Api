using Moq;
using OLab.Access;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System.Security.Claims;

namespace OLab.Test;

public class AuthenticatedContextTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly TestAuthenticatedContext _userContext;

  public AuthenticatedContextTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _userContext = new TestAuthenticatedContext( _mockLogger.Object, _mockDbContext.Object );
  }

  [Fact]
  public void Constructor_WithValidParameters_InitializesProperties()
  {
    Assert.NotNull( _userContext.GetLogger() );
    Assert.NotNull( _userContext.GetDbContext() );
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

  private class TestAuthenticatedContext : AuthenticatedContext
  {
    public TestAuthenticatedContext(IOLabLogger logger, OLabDBContext dbContext) : base( logger, dbContext ) { }

    public new void SetClaims(IDictionary<string, string> claims) => base.SetClaims( claims );
    public new string GetClaim(string key, bool isRequired = true) => base.GetClaim( key, isRequired );
    public new void LoadUserContext() => base.LoadUserContext();
    public new IOLabLogger GetLogger() => base.GetLogger();
    public new OLabDBContext GetDbContext() => base.GetDbContext();
  }
}
