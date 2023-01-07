using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Data;
using OLabWebAPI.Model;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;
using OLabWebAPI.Utils;
using System;
using System.Threading.Tasks;
using Dawn;

namespace OLabWebAPI.Services.TurkTalk
{
  // [Route("olab/api/v3/turktalk")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public partial class TurkTalkHub : Hub
  {
    private readonly OLabLogger _logger;
    private readonly Conference _conference;
    protected readonly OLabDBContext DbContext;

    public string ContextId { get; set; }

    /// <summary>
    /// TurkTalkHub constructor
    /// </summary>
    /// <param name="logger">Dependancy-injected logger</param>
    public TurkTalkHub(ILogger<TurkTalkHub> logger, OLabDBContext dbContext, Conference conference)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));
      Guard.Argument(conference).NotNull(nameof(conference));

      _conference = conference ?? throw new ArgumentNullException(nameof(conference));
      _logger = new OLabLogger(logger);

      DbContext = dbContext;

      _logger.LogDebug($"TurkTalkHub ctor");
    }

    /// <summary>
    /// Broadcast message to all participants
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    public void BroadcastMessage(string sender, string message)
    {
      try
      {
        _logger.LogDebug($"Broadcast message received from '{sender}': '{message}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"BroadcastMessage exception: {ex.Message}");
      }
    }

  }
}
