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
        // extract fields from bearer token
        var identity = (ClaimsIdentity)Context.User.Identity;
        var nickName = identity.FindFirst("name").Value;
        var userId = identity.FindFirst(ClaimTypes.Name).Value;

        Guard.Argument(userId).NotEmpty(userId);
        Guard.Argument(topicName).NotEmpty(topicName);

        var moderator = new ModeratorGroupName(topicName, userId, nickName);
        _logger.LogInformation($"RegisterModerator: '{moderator.ToString()}'");

        var room = _conference.GetCreateUnmoderatedTopicRoom(topicName);
        await room.AddModeratorAsync(moderator, Context.ConnectionId);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }
    }
  }
}
