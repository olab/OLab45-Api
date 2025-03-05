using Moq;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Test.Utils;

namespace OLab.Test.OLabAuthorization;

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

    _authorization = new Access.OLabAuthorization(
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
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    // Assert
    Assert.Equal( testUser.Id, _authorization.OLabUser.Id );
    Assert.Equal( testUser.Username, _authorization.OLabUser.Username );
    Assert.Equal( "olab", _authorization.Issuer );
    Assert.Equal( testUser.UserGrouproles.Count, _authorization.UsersGroupRoles.Count );
    Assert.True( _authorization.GroupRoleAcls.Count > 0 );
  }

  [Fact]
  public async Task OLabSuperuserUser_AccessToAllGroup()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UsersMultipleGroups.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    // Act
    var physGroup = _mockDbContext.Object.Groups.First( x => x.Name == "olab" );
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    var result = await _authorization.GetAuthorizedUserGroupsAsync();

    // Assert
    Assert.Equal( _mockDbContext.Object.Groups.Count(), result.Count() );
  }

  [Fact]
  public async Task OLabSuperuserUser_AccessToAllUsers()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UsersMultipleGroups.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabSuperuserContext.json" ).First();

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    var users = _mockDbContext.Object.Users.ToList();
    var groups = await _authorization.GetAuthorizedUserGroupsAsync();

    var filteredUsers = users.Where( x => x.UserGrouproles.Any( y => groups.Any( z => z.Id == y.GroupId ) ) ).ToList();

    // Assert
    Assert.Equal( users.Count(), filteredUsers.Count() );
  }

  [Fact]
  public async Task OLabGroupSuperuser_AccessToOnlyGroupUsers()
  {
    var testUser = TestUtilities.MoqUsersFromJson( _mockDbContext, "json\\UsersMultipleGroups.json" ).First();
    var authenticatedContext = TestUtilities.BuildQueryableListFromJson<MoqAuthenticatedContext>( "json\\UserOLabTestGroupSuperuserContext.json" ).First();

    // Act
    await _authorization.ApplyUserContextAsync( authenticatedContext );

    var users = _mockDbContext.Object.Users.ToList();
    var groups = await _authorization.GetAuthorizedUserGroupsAsync();

    var filteredUsers = users.Where( x => x.UserGrouproles.Any( y => groups.Any( z => z.Id == y.GroupId ) ) ).ToList();

    // Assert
    Assert.Single( filteredUsers );
  }
}
