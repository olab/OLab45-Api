using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
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
    /// <param name="isbot">Moderator is a bot</param>
    public void RegisterModerator(string moderatorName, string roomName, bool isBot)
    {
      try
      {
        _logger.LogInformation($"RegisterModerator: '{moderatorName}', atrium {roomName}");

        var participant = new Participant(moderatorName, Context.ConnectionId);
        _conference.AddModerator( participant, roomName, isBot );
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }

    }

    /// <summary>
    /// Assignment of attendees to atrium
    /// </summary>
    /// <param name="attendee">Attendee to assign</param>
    /// <param name="atriumName">Target atrium</param>
    public void AssignAttendee(Participant attendee, string atriumName)
    {
      try
      {
        _logger.LogInformation($"AssignAttendee: '{attendee}'");

        _conference.AssignAttendee(Context.ConnectionId, attendee, atriumName);
      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendee exception: {ex.Message}");
      }

    }
  }
}
