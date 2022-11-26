using System;
using System.Threading.Tasks;
using Dawn;
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
    /// <param name="topicName">Topic id</param>
    /// <param name="isbot">Moderator is a bot</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task RegisterModerator(string topicName, string sessionId, bool isBot)
    {
      try
      {
        string moderatorName = Context.User.Identity.Name;

        Guard.Argument(moderatorName).NotEmpty(moderatorName);
        Guard.Argument(topicName).NotEmpty(topicName);

        _logger.LogInformation($"RegisterModerator: '{moderatorName}', session {topicName}' sessionId {sessionId}, isBot {isBot}");

        var room = _conference.GetCreateUnmoderatedTopicRoom(topicName);
        await room.AddModeratorAsync(moderatorName, Context.ConnectionId);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }
    }
  }
}
