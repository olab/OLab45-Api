using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OLab.Common.Utils;

namespace OLab.Test
{
  public class OLabLoggerTests
  {
    private readonly OLabLogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    public OLabLoggerTests()
    {
      _loggerFactory = new NullLoggerFactory();
      _logger = new OLabLogger( _loggerFactory, true );
    }

    [Fact]
    public void LogDebug_MessageIsLogged()
    {
      var message = "Debug message";
      _logger.LogDebug( message );
      var messages = _logger.GetMessages( OLabLogMessage.MessageLevel.Debug );
      Assert.Contains( messages, m => m.Message == message && m.Level == OLabLogMessage.MessageLevel.Debug );
    }

    [Fact]
    public void LogFatal_MessageIsLogged()
    {
      var message = "Fatal message";
      _logger.LogFatal( message );
      var messages = _logger.GetMessages( OLabLogMessage.MessageLevel.Fatal );
      Assert.Contains( messages, m => m.Message == message && m.Level == OLabLogMessage.MessageLevel.Fatal );
    }

    [Fact]
    public void LogError_ExceptionIsLogged()
    {
      var exception = new Exception( "Test exception" );
      _logger.LogError( exception );
      var messages = _logger.GetMessages( OLabLogMessage.MessageLevel.Error );
      Assert.Contains( messages, m => m.Message.Contains( "ERROR: Test exception" ) && m.Level == OLabLogMessage.MessageLevel.Error );
    }

    [Fact]
    public void LogInformation_MessageIsLogged()
    {
      var message = "Information message";
      _logger.LogInformation( message );
      var messages = _logger.GetMessages( OLabLogMessage.MessageLevel.Info );
      Assert.Contains( messages, m => m.Message == message && m.Level == OLabLogMessage.MessageLevel.Info );
    }

    [Fact]
    public void LogWarning_MessageIsLogged()
    {
      var message = "Warning message";
      _logger.LogWarning( message );
      var messages = _logger.GetMessages( OLabLogMessage.MessageLevel.Warn );
      Assert.Contains( messages, m => m.Message == message && m.Level == OLabLogMessage.MessageLevel.Warn );
    }

    [Fact]
    public void HaveFatalError_ReturnsTrueIfFatalErrorExists()
    {
      _logger.LogFatal( "Fatal error" );
      Assert.True( _logger.HaveFatalError );
    }

    [Fact]
    public void HaveFatalError_ReturnsFalseIfNoFatalErrorExists()
    {
      _logger.LogError( "Error message" );
      Assert.False( _logger.HaveFatalError );
    }

    [Fact]
    public void HasErrorMessage_ReturnsTrueIfErrorExists()
    {
      _logger.LogError( "Error message" );
      Assert.True( _logger.HasErrorMessage() );
    }

    [Fact]
    public void HasErrorMessage_ReturnsFalseIfNoErrorExists()
    {
      _logger.LogDebug( "Debug message" );
      Assert.False( _logger.HasErrorMessage() );
    }

    [Fact]
    public void Clear_RemovesAllMessages()
    {
      _logger.LogDebug( "Debug message" );
      _logger.Clear();
      var messages = _logger.GetMessages();
      Assert.Empty( messages );
    }
  }
}
