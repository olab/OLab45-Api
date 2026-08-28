using Moq;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.ReaderWriters;
using OLab.Test.Utils;

namespace OLab.Test.ReaderWriters;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

public class NullNumberTests
{
  private readonly IOLabLogger _logger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  public readonly Mock<IOLabConfiguration> _mockConfiguration;

  private readonly GroupRoleAclReaderWriter _groupRoleAclReaderWriter;

  public NullNumberTests()
  {
    _logger = new Mock<IOLabLogger>().Object;
    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();

    TestUtilities.MoqGroupRoleAclFromJsonFile( _mockDbContext, "json\\GroupRoleAclsApp.json" );

    _groupRoleAclReaderWriter = new GroupRoleAclReaderWriter( _logger, _mockDbContext.Object );
  }

  [Fact]
  public async Task GetAsync_ReturnsAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetAsync();
    Assert.Equal( 10, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_nnAn_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, Constants.ScopeLevelApp, null );
    Assert.Equal( 8, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_nnA1_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, null, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 4, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_n1An_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, 1, Constants.ScopeLevelApp, null );
    Assert.Equal( 4, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_n1A1_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( null, 1, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 2, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_1nAn_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, null, Constants.ScopeLevelApp, null );
    Assert.Equal( 4, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_1nA1_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, null, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 2, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_11An_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 1, Constants.ScopeLevelApp, null );
    Assert.Equal( 2, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_11A1_ReturnsOnlyAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 1, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 1, result.Count() );
  }

}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
