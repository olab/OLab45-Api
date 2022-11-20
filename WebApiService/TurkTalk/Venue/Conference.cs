using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Dawn;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
  public class Conference
  {
    private readonly ILogger _logger;
    private IDictionary<string, Session> _sessions;

    public Conference(ILogger logger)
    {
      Guard.Argument(logger).NotNull(nameof(logger));

      this._logger = logger;
      _sessions = new ConcurrentDictionary<string, Session>();      
    }

    public ILogger Logger { get { return _logger; }}
  }
}