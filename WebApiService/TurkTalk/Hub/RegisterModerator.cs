using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Register moderator to atrium
    /// </summary>
    /// <param name="moderatorName">Moderator's name</param>
    /// <param name="roomName">Atrium name</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="isbot">Moderator is a bot</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public void RegisterModerator(string moderatorName, string sessionName, string sessionId, bool isBot)
    {
      try
      {
        _logger.LogInformation($"RegisterModerator: '{moderatorName}', session {sessionName}' sessionId {sessionId}, isBot {isBot}");

        
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }

    }
  }
}
