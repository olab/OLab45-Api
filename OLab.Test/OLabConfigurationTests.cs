using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OLab.Common.Utils;

namespace OLab.Test
{
  public class OLabConfigurationTests
  {
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public OLabConfigurationTests()
    {
      _mockLoggerFactory = new Mock<ILoggerFactory>();
      _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ShouldThrowArgumentNullException()
    {
      // Arrange
      ILoggerFactory? loggerFactory = null;

      // Act & Assert
      Assert.Throws<ArgumentNullException>( () => new OLabConfiguration( loggerFactory, _mockConfiguration.Object ) );
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
      // Arrange
      IConfiguration? configuration = null;

      // Act & Assert
      Assert.Throws<ArgumentNullException>( () => new OLabConfiguration( _mockLoggerFactory.Object, configuration ) );
    }

    [Fact]
    public void GetRawConfiguration_ShouldReturnConfiguration()
    {
      // Arrange
      var config = new OLabConfiguration( _mockLoggerFactory.Object, _mockConfiguration.Object );

      // Act
      var result = config.GetRawConfiguration();

      // Assert
      Assert.Equal( _mockConfiguration.Object, result );
    }

  }
}
