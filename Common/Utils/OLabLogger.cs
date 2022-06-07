using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI.Utils
{
  public class OLabLogMessage
  {
    public enum MessageLevel
    {
      Debug = 0,
      Info,
      Warn,
      Error,
      Fatal
    };

    public string ObjectType;
    public int ObjectIndex;
    public MessageLevel Level;
    public string LevelStr { get { return Level.ToString(); } }
    public string Message;
    public DateTime Time;

    public override string ToString()
    {
      return $"{Level}: {ObjectType}({ObjectIndex}): {Message}";
    }
  }

  public class OLabLogger
  {
    private readonly IList<OLabLogMessage> _messages = new List<OLabLogMessage>();
    private readonly ILogger _logger;
    private readonly bool _keepMessages;

    public ILogger GetLogger() { return _logger; }

    // for cases where we don't have/need an actual ILogger
    public OLabLogger(bool keepMessages = false)
    {
      _keepMessages = keepMessages;
    }

    public OLabLogger(ILogger logger, bool keepMessages = true )
    {
      _logger = logger;
      _keepMessages = keepMessages;

    }

    public bool HaveFatalError => _messages.Any(x => x.Level == OLabLogMessage.MessageLevel.Fatal);

    public IList<OLabLogMessage> GetMessages(OLabLogMessage.MessageLevel level = OLabLogMessage.MessageLevel.Debug)
    {
      return _messages.Where(x => x.Level >= level).ToList();
    }

    private void Log(OLabLogMessage.MessageLevel level, string message)
    {
      if (_keepMessages)
      {
        OLabLogMessage obj = new OLabLogMessage
        {
          Message = message,
          Time = DateTime.Now,
          Level = level
        };
        _messages.Add(obj);
      }
    }

    private void Log(OLabLogMessage.MessageLevel level, string type, int index, string message)
    {
      OLabLogMessage obj = new OLabLogMessage
      {
        ObjectIndex = index,
        ObjectType = type,
        Message = message,
        Time = DateTime.Now,
        Level = level
      };
      _messages.Add(obj);
    }

    public void LogDebug(string message)
    {
      if (_logger != null)
        _logger.LogDebug(message);
      Log(OLabLogMessage.MessageLevel.Debug, message);
    }

    public void LogDebug(string type, int index, string message)
    {
      if (_logger != null)
        _logger.LogDebug(message);
      Log(OLabLogMessage.MessageLevel.Debug, type, index, message);
    }

    public void LogFatal(string message)
    {
      if (_logger != null)
        _logger.LogCritical(message);
      Log(OLabLogMessage.MessageLevel.Fatal, message);
    }

    public void LogFatal(string type, int index, string message)
    {
      if (_logger != null)
        _logger.LogCritical(message);
      Log(OLabLogMessage.MessageLevel.Fatal, type, index, message);
    }

    public void LogError(Exception ex, string message)
    {
      if (_logger != null)
        _logger.LogError(ex, message);
      Log(OLabLogMessage.MessageLevel.Error, message);
    }

    public void LogError(string type, int index, Exception ex, string message)
    {
      if (_logger != null)
        _logger.LogError(ex, message);
      Log(OLabLogMessage.MessageLevel.Error, type, index, message);
    }

    public void LogError(string message)
    {
      if (_logger != null)
        _logger.LogError(message);
      Log(OLabLogMessage.MessageLevel.Error, message);
    }

    public void LogError(string type, int index, string message)
    {
      if (_logger != null)
        _logger.LogError(message);
      Log(OLabLogMessage.MessageLevel.Error, type, index, message);
    }

    public void LogInformation(string message)
    {
      if (_logger != null)
        _logger.LogInformation(message);
      Log(OLabLogMessage.MessageLevel.Info, message);
    }

    public void LogWarning(string message)
    {
      if (_logger != null)
        _logger.LogWarning(message);
      Log(OLabLogMessage.MessageLevel.Warn, message);
    }

    public void LogInformation(string type, int index, string message)
    {
      if (_logger != null)
        _logger.LogInformation(message);
      Log(OLabLogMessage.MessageLevel.Info, type, index, message);
    }

    public void LogWarning(string type, int index, string message)
    {
      if (_logger != null)
        _logger.LogWarning(message);
      Log(OLabLogMessage.MessageLevel.Warn, type, index, message);
    }

  }

}