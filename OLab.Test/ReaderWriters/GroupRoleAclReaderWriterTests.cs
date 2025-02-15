using Moq;
using OLab.Api.Data.Exceptions;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.Model;
using OLab.Data.ReaderWriters;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OLab.Test;

public class GroupRoleAclReaderWriterTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<RoleReaderWriter> _mockRoleReaderWriter;
  private readonly GroupRoleAclReaderWriter _groupRoleAclReaderWriter;

  public GroupRoleAclReaderWriterTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _mockRoleReaderWriter = new Mock<RoleReaderWriter>( _mockLogger.Object, _mockDbContext.Object );
    _groupRoleAclReaderWriter = GroupRoleAclReaderWriter.Instance( _mockLogger.Object, _mockDbContext.Object );
  }

  [Fact]
  public async Task EditAsync_ValidInput_UpdatesGroupRoleAcl()
  {
    var acl = new GrouproleAcls { Id = 1, GroupId = 1, RoleId = 1, Acl2 = 7 };
    _mockDbContext.Setup( db => db.GrouproleAcls.Update( acl ) );
    _mockDbContext.Setup( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ) ).ReturnsAsync( 1 );
    var result = await _groupRoleAclReaderWriter.EditAsync( acl, true );

    Assert.Equal( acl, result );
    _mockDbContext.Verify( db => db.GrouproleAcls.Update( acl ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ), Times.Once );
  }

  [Fact]
  public async Task CreateAsync_ValidInput_CreatesGroupRoleAcl()
  {
    var acl = new GrouproleAcls { Id = 1, GroupId = 1, RoleId = 1, Acl2 = 7 };
    _mockDbContext.Setup( db => db.GrouproleAcls.AddAsync( acl, It.IsAny<CancellationToken>() ) ).Returns( Task.CompletedTask );
    _mockDbContext.Setup( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ) ).Returns( Task.CompletedTask );

    var result = await _groupRoleAclReaderWriter.CreateAsync( acl, true );

    Assert.Equal( acl, result );
    _mockDbContext.Verify( db => db.GrouproleAcls.AddAsync( acl, It.IsAny<CancellationToken>() ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ), Times.Once );
  }

  [Fact]
  public async Task CreateDefaultAclsForGroupAsync_ValidGroupId_CreatesDefaultAcls()
  {
    var groupId = 1u;
    var authorRole = new Roles { Id = 1, Name = "author" };
    var learnerRole = new Roles { Id = 2, Name = "learner" };
    var superUserRole = new Roles { Id = 3, Name = Roles.SuperUserRole };
    var directorRole = new Roles { Id = 4, Name = Roles.DirectorRole };
    var adminRole = new Roles { Id = 5, Name = "administrator" };

    _mockRoleReaderWriter.Setup( rw => rw.GetAsync( "author" ) ).ReturnsAsync( authorRole );
    _mockRoleReaderWriter.Setup( rw => rw.GetAsync( "learner" ) ).ReturnsAsync( learnerRole );
    _mockRoleReaderWriter.Setup( rw => rw.GetAsync( Roles.SuperUserRole ) ).ReturnsAsync( superUserRole );
    _mockRoleReaderWriter.Setup( rw => rw.GetAsync( Roles.DirectorRole ) ).ReturnsAsync( directorRole );
    _mockRoleReaderWriter.Setup( rw => rw.GetAsync( "administrator" ) ).ReturnsAsync( adminRole );

    await _groupRoleAclReaderWriter.CreateDefaultAclsForGroupAsync( groupId );

    _mockDbContext.Verify( db => db.GrouproleAcls.AddAsync( It.IsAny<GrouproleAcls>() ), Times.Exactly( 6 ) );
  }

  [Fact]
  public async Task GetAsync_ReturnsAllGroupRoleAcls()
  {
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1 }, new GrouproleAcls { Id = 2 } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Include( "Group" ).Include( "Role" ).ToListAsync() ).ReturnsAsync( acls );

    var result = await _groupRoleAclReaderWriter.GetAsync();

    Assert.Equal( acls, result );
  }

  [Fact]
  public async Task GetListAsync_ReturnsFilteredGroupRoleAcls()
  {
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1 }, new GrouproleAcls { Id = 2 } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Include( "Group" ).Include( "Role" ).ToListAsync() ).ReturnsAsync( acls );

    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 1, "type", new List<uint?> { 1 } );

    Assert.Equal( acls, result );
  }

  [Fact]
  public void GetByUserGroupRoles_ReturnsGroupRoleAcls()
  {
    var groupRoles = new List<UserGrouproles> { new UserGrouproles { GroupId = 1, RoleId = 1 } };
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1, GroupId = 1, RoleId = 1 } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Where( It.IsAny<Func<GrouproleAcls, bool>>() ).ToList() ).Returns( acls );

    var result = _groupRoleAclReaderWriter.GetByUserGroupRoles( groupRoles );

    Assert.Equal( acls, result );
  }

  [Fact]
  public void GetForGroup_ReturnsGroupRoleAcls()
  {
    var groupId = 1u;
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1, GroupId = groupId } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Where( It.IsAny<Func<GrouproleAcls, bool>>() ).ToList() ).Returns( acls );

    var result = _groupRoleAclReaderWriter.GetForGroup( groupId );

    Assert.Equal( acls, result );
  }

  [Fact]
  public async Task DeleteAsync_ValidId_DeletesGroupRoleAcl()
  {
    var acl = new GrouproleAcls { Id = 1 };
    _mockDbContext.Setup( db => db.GrouproleAcls.FirstOrDefaultAsync( It.IsAny<Func<GrouproleAcls, bool>>() ) ).ReturnsAsync( acl );
    _mockDbContext.Setup( db => db.GrouproleAcls.Remove( acl ) );
    _mockDbContext.Setup( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ) ).Returns( Task.CompletedTask );

    var result = await _groupRoleAclReaderWriter.DeleteAsync( 1, true );

    Assert.Equal( 1u, result.Id );
    Assert.Equal( "Deleted", result.Message );
    _mockDbContext.Verify( db => db.GrouproleAcls.Remove( acl ), Times.Once );
    _mockDbContext.Verify( db => db.SaveChangesAsync(), Times.Once );
  }

  [Fact]
  public void GetByGroupRole_ReturnsGroupRoleAcl()
  {
    var acl = new GrouproleAcls { Id = 1, Group = new Groups { Name = "group" }, Role = new Roles { Name = "role" } };
    _mockDbContext.Setup( db => db.GrouproleAcls.FirstOrDefault( It.IsAny<Func<GrouproleAcls, bool>>() ) ).Returns( acl );

    var result = _groupRoleAclReaderWriter.GetByGroupRole( "group", "role" );

    Assert.Equal( acl, result );
  }

  [Fact]
  public void GetByGroup_ReturnsGroupRoleAcls()
  {
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1, Group = new Groups { Name = "group" } } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Where( It.IsAny<Func<GrouproleAcls, bool>>() ).ToList() ).Returns( acls );

    var result = _groupRoleAclReaderWriter.GetByGroup( "group" );

    Assert.Equal( acls, result );
  }

  [Fact]
  public void GetByRole_ReturnsGroupRoleAcls()
  {
    var acls = new List<GrouproleAcls> { new GrouproleAcls { Id = 1, Role = new Roles { Name = "role" } } };
    _mockDbContext.Setup( db => db.GrouproleAcls.Where( It.IsAny<Func<GrouproleAcls, bool>>() ).ToList() ).Returns( acls );

    var result = _groupRoleAclReaderWriter.GetByRole( "role" );

    Assert.Equal( acls, result );
  }
}
