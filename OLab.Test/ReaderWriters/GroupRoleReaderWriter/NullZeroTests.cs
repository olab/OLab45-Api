using Moq;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using OLab.Test.Utils;

namespace OLab.Test.ReaderWriters;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

public class NullZeroTests
{
  private readonly IOLabLogger _logger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  public readonly Mock<IOLabConfiguration> _mockConfiguration;

  private readonly GroupRoleAclReaderWriter _groupRoleAclReaderWriter;

  public NullZeroTests()
  {
    _logger = new Mock<IOLabLogger>().Object;
    _mockDbContext = new Mock<OLabDBContext>();
    _mockConfiguration = new Mock<IOLabConfiguration>();

    TestUtilities.MoqGroupRoleAclFromJsonFile( _mockDbContext, "json\\GroupRoleAclsApp.json" );

    _groupRoleAclReaderWriter = new GroupRoleAclReaderWriter( _logger, _mockDbContext.Object );
  }

  [Fact]
  public async Task GetAsync_ReturnsAllAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetAsync();
    Assert.Equal( 10, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_00A0_ReturnsAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 0, 0, Constants.ScopeLevelApp, new List<uint?> { 0 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_00A1_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 0, 0, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_01A0_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 0, 1, Constants.ScopeLevelApp, new List<uint?> { 0 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_01A1_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 0, 1, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_10A0_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 0, Constants.ScopeLevelApp, new List<uint?> { 0 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_10A1_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 0, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_11A0_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 1, Constants.ScopeLevelApp, new List<uint?> { 0 } );
    Assert.Equal( 1, result.Count() );
  }

  [Fact]
  public async Task GetListAsyncCase_11A1_ReturnsFilteredAppAcls()
  {
    var result = await _groupRoleAclReaderWriter.GetListAsync( 1, 1, Constants.ScopeLevelApp, new List<uint?> { 1 } );
    Assert.Equal( 1, result.Count() );
  }

}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
