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
    /// <param name="sessionId">Session id</param>
    /// <param name="isbot">Moderator is a bot</param>
    public void RegisterModerator(string moderatorName, string roomName, string sessionId, bool isBot)
    {
      try
      {
        _logger.LogInformation($"RegisterModerator: '{moderatorName}', room {roomName}");

        var moderator = new Participant
        {
          Name = moderatorName
        };

        if ( !string.IsNullOrEmpty( sessionId ))
          moderator.SessionId = sessionId;
        
        _conference.AddModerator( Context.ConnectionId, moderator, roomName, isBot );
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");
      }

    }

    /// <summary>
    /// Assignment of attendees to atrium
    /// </summary>
    /// <param name="requestingSessionId">SessionId that requested the assign</param>
    /// <param name="attendee">Attendee to assign</param>
    /// <param name="atriumName">Target atrium</param>
    public void AssignAttendee(string requestingSessionId, Participant attendee, string atriumName)
    {
      try
      {
        _logger.LogInformation($"AssignAttendee: '{attendee}'");        
        _conference.AssignAttendee(Context.ConnectionId, requestingSessionId, attendee, atriumName);
      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendee exception: {ex.Message}");
      }

    }
  }
}
