using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  // [Route("olab/api/v3/turktalk")]
  public partial class TurkTalkHub : Hub
  {

    /// <summary>
    /// Handle new Attendee registration to room
    /// </summary>
    /// <param name="attendeeName">Attendee name</param>
    /// <param name="roomName">Atrium name</param>
    /// <param name="sessionId">Atrium name</param>
    public void RegisterAttendee(string attendeeName, string roomName, string sessionId )
    {
      try
      {
        _logger.LogInformation($"RegisterAttendee: '{attendeeName}', room '{roomName}'");

        var participant = new Participant
        {
          Name = attendeeName
        };

        if ( !string.IsNullOrEmpty( sessionId ))
          participant.SessionId = sessionId;
          
        _conference.AddAttendee( Context.ConnectionId, participant, roomName );
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }

    }

  }
}
