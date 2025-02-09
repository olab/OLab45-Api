using Moq;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using OLab.Data.Mappers;
using OLab.Data.ReaderWriters;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using OLab.Api.Utils;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using OLab.Common.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using OLab.Test.Utils;

namespace OLab.Test;

public class GroupRoleAclsEndpointTests
{
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<IOLabModuleProvider<IWikiTagModule>> _mockWikiTagProvider;
  private readonly Mock<IOLabModuleProvider<IFileStorageModule>> _mockFileStorageProvider;
  private readonly Mock<GroupRoleAclReaderWriter> _mockReaderWriter;
  private readonly Mock<GroupRoleAclMapper> _mockMapper;
  private readonly IList<GrouproleAcls> _groupRoleAcls;
  private readonly GroupRoleAclsEndpoint _endpoint;

  private readonly OLabLogger _logger;
  private readonly ILoggerFactory _loggerFactory;

  public GroupRoleAclsEndpointTests()
  {
    _loggerFactory = new NullLoggerFactory();
    _logger = new OLabLogger( _loggerFactory, true );

    var builder = new ConfigurationBuilder()
        .SetBasePath( Directory.GetCurrentDirectory() )
        .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
        .AddEnvironmentVariables();

    IConfiguration config = builder.Build();

    var olabConfig = new OLabConfiguration( _loggerFactory, config );

    _mockDbContext = new Mock<OLabDBContext>();
    _mockWikiTagProvider = new Mock<IOLabModuleProvider<IWikiTagModule>>();
    _mockFileStorageProvider = new Mock<IOLabModuleProvider<IFileStorageModule>>();
    _mockReaderWriter = new Mock<GroupRoleAclReaderWriter>( _logger, _mockDbContext.Object );
    _mockMapper = new Mock<GroupRoleAclMapper>( _logger, _mockDbContext.Object );

    _groupRoleAcls = TestUtilities.MoqGroupRoleAclFromJsonFile( _mockDbContext, "json\\GroupRoleAcls.json" );
    TestUtilities.MoqGroupsFromJsonFile( _mockDbContext, "json\\Groups.json" );
    TestUtilities.MoqRoleFromJsonFile( _mockDbContext, "json\\Roles.json" );
    TestUtilities.MoqSystemApplicationsFromJsonFile( _mockDbContext, "json\\SystemApplications.json" );

    _endpoint = new GroupRoleAclsEndpoint(
        _logger,
        olabConfig,
        _mockDbContext.Object,
        _mockWikiTagProvider.Object,
        _mockFileStorageProvider.Object );
  }

  [Fact]
  public async Task GetAsync_NoFilters_ReturnsAllAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest();

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Equal( _groupRoleAcls.Count, result.Count );
  }

  [Fact]
  public async Task GetAsync_WithGroupId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest
    {
      GroupId = 1,
      RoleId = null
    };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Equal( _groupRoleAcls.Count( x => x.GroupId == model.GroupId ), result.Count );
  }

  [Fact]
  public async Task GetAsync_WithRoleId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest
    {
      GroupId = null,
      RoleId = 6
    };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Equal( 4, result.Count );
  }

  [Fact]
  public async Task GetAsync_WithNullMapId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { MapIds = new List<uint?> { null } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Equal( 4, result.Count() );
  }

  [Fact]
  public async Task GetAsync_WithNotNullMapId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { MapIds = new List<uint?> { 0 } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Empty( result );
  }

  [Fact]
  public async Task GetAsync_WithAppId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { AppIds = new List<uint?> { 9 } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Single( result );
  }

  [Fact]
  public async Task GetAsync_WithNotNullAppId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { AppIds = new List<uint?> { 0 } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Equal( 3, result.Count );
  }

  [Fact]
  public async Task GetAsync_WithNullNodeId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { NodeIds = new List<uint?> { null } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Empty( result );
  }

  [Fact]
  public async Task GetAsync_WithNullAppId_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { AppIds = new List<uint?> { null } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.True( result.Count() == 0 );
  }

  [Fact]
  public async Task GetAsync_WithAppIds_ReturnsFilteredAcls()
  {
    // Arrange
    var model = new GroupRoleAclRequest { AppIds = new List<uint?> { 8 } };

    // Act
    var result = await _endpoint.GetAsync( null, model );

    // Assert
    Assert.Single( result );
  }
}
