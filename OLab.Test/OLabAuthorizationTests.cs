using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using NuGet.Packaging;
using OLab.Access;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
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
  private readonly Mock<IUserContext> _mockUserContext;
  private readonly OLabAuthorization _authorization;
  private readonly Api.Model.Users _testUser1;

  public OLabAuthorizationTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();
    _mockUserContext = new Mock<IUserContext>();

    var groupRoleList = TestUtilities.LoadRecordsFromJson<GrouproleAcls>( "json\\GroupRoleAcls.json" );

    var mockSet = new Mock<DbSet<GrouproleAcls>>();
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.Provider ).Returns( groupRoleList.Provider );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.Expression ).Returns( groupRoleList.Expression );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.ElementType ).Returns( groupRoleList.ElementType );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.GetEnumerator() ).Returns( () => groupRoleList.GetEnumerator() );

    _mockDbContext.Setup( c => c.GrouproleAcls ).Returns( mockSet.Object );

    _authorization = new OLabAuthorization(
        _mockLogger.Object,
        _mockDbContext.Object,
        _mockConfiguration.Object
    );

    _testUser1 = TestUtilities.LoadRecordsFromJson<Users>( "json\\UserAStevan.json" ).First();
  }

  [Fact]
  public void ApplyUserContext_WithValidUser_SetsProperties()
  {
    // Act
    _authorization.ApplyUserContext( _testUser1 );

    // Assert
    Assert.Equal( _testUser1, _authorization.OLabUser );
    Assert.Equal( "olab", _authorization.Issuer );
    Assert.Equal( _testUser1.UserGrouproles.Count(), _authorization.UserGroupRoles.Count() );
    Assert.Equal( 8, _authorization.GroupRoleAcls.Count() );
  }

}
