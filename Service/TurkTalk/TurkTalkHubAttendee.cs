using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  // [Route("olab/api/v3/turktalk")]
  public partial class TurkTalkHub : Hub
  {

    /// <summary>
    /// Handle new Attendee registration to room
    /// </summary>
    /// <param name="name">Attendee name</param>
    /// <param name="atriumName">Atrium name (from WikiTag)</param>
    /// <param name="sessionId">Optional id from previous assigned session</param>
    public void RegisterAttendee(string name, string atriumName, string sessionId = null)
    {
      try
      {
        if (string.IsNullOrEmpty(name))
          throw new ArgumentNullException(nameof(name));

        if (string.IsNullOrEmpty(atriumName))
          throw new ArgumentNullException(nameof(atriumName));

        _logger.LogInformation($"RegisterAttendee: '{name}', atrium '{atriumName}', sessionId '{sessionId}'");

        var atrium = Conference.GetAtriumByName(atriumName, true);

        // see if attendee was already known (in a room or atrium)
        var attendee = atrium.GetAttendee(sessionId);

        // if attendee is new, generate Participant and add.  otherwise
        // update the connectionId for attendee.
        if (attendee == null)
        {
          attendee = new Participant(name, Context.ConnectionId);
          atrium.AddAttendee(attendee);
        }
        else
          atrium.UpdateAttendeeConnection(attendee, Context.ConnectionId);

      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }

    }

  }
}
