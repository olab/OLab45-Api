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

public class OLabAuthorizationTests
{
  public readonly IOLabLogger _logger;
  public readonly Mock<OLabDBContext> _mockDbContext;
  public readonly Mock<IOLabConfiguration> _mockConfiguration;
  public readonly IOLabAuthorization _authorization;
  public readonly IAuthenticatedContext _authenticatedContext;


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
}
