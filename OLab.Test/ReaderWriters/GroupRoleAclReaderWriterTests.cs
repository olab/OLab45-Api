using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using OLab.Api.Data.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Model;
using OLab.Data.ReaderWriters;
using OLab.Test.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OLab.Test.ReaderWriters;

public class GroupRoleAclReaderWriterTests
{
  private readonly IOLabLogger _logger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  public readonly Mock<IOLabConfiguration> _mockConfiguration;

  private readonly GroupRoleAclReaderWriter _groupRoleAclReaderWriter;
  private readonly Mock<RoleReaderWriter> _mockRoleReaderWriter;

  public GroupRoleAclReaderWriterTests()
  {
    _logger = new Mock<IOLabLogger>().Object;
    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();

    TestUtilities.MoqGroupRoleAclFromJsonFile( _mockDbContext, "json\\GroupRoleAclsSuite.json" );

    _mockRoleReaderWriter = new Mock<RoleReaderWriter>( _logger, _mockDbContext.Object );
    _groupRoleAclReaderWriter = new GroupRoleAclReaderWriter( _logger, _mockDbContext.Object );
  }

  [Fact]
  public async Task EditAsync_ValidAcl_ReturnsEditedAcl()
  {
    var acl = new GrouproleAcls { Id = 1, GroupId = 1, RoleId = 1, Acl2 = 7 };
    _mockDbContext.Setup( db => db.GrouproleAcls.Update( acl ) );
    _mockDbContext.Setup( db => db.SaveChangesAsync( It.IsAny<CancellationToken>() ) ).ReturnsAsync( 1 );

    var result = await _groupRoleAclReaderWriter.EditAsync( acl, true );

    Assert.Equal( acl, result );
  }

  [Fact]
  public async Task GetAsync_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetAsync();
    Assert.Equal( 54, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_NNNN_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, null, null );
    Assert.Equal( 54, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_NNMN_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, Constants.ScopeLevelMap, null );
    Assert.Equal( 27, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCaseNNM0_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, Constants.ScopeLevelMap, new List<uint?> { 0 } );
    Assert.Equal( 9, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCaseNNM1_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, Constants.ScopeLevelMap, new List<uint?> { 1 } );
    Assert.Equal( 27, result.Count() );
  }

}
