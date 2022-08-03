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
    /// <param name="atriumName">Atrium name</param>
    /// <param name="sessionId">Atrium name</param>
    public void RegisterAttendee(string attendeeName, string atriumName, string sessionId )
    {
      try
      {
        _logger.LogInformation($"RegisterAttendee: '{attendeeName}', atrium '{atriumName}'");

        var participant = new Participant(attendeeName, Context.ConnectionId);
        _conference.AddAttendee( participant, atriumName );
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }

    }

  }
}
