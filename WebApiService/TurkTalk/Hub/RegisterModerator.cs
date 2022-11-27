using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;

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
    public async Task RegisterModerator(string topicName, bool isBot)
    {
      try
      {
        var moderator = new Moderator(topicName, Context);
        var room = _conference.GetCreateUnmoderatedTopicRoom(topicName);

        // add room index to moderator info
        moderator.AssignToRoom(room.Index);
        _logger.LogInformation($"RegisterModerator: '{moderator.ToString()}'");

        await room.AddModeratorAsync(moderator);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }
    }
  }
}
