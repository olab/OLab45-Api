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
    /// <param name="attendeeName">Attendee name</param>
    /// <param name="atriumName">Atrium name</param>
    public void RegisterAttendee(string attendeeName, string atriumName)
    {
      try
      {
        _logger.LogInformation($"RegisterAttendee: '{attendeeName}', atrium '{atriumName}'");

        var participant = new Participant(attendeeName, Context.ConnectionId);
        var atrium = Conference.GetAtriumByName(atriumName, true);
        if ( atrium == null )
          throw new Exception($"Cannot find atrium '{atrium}'");

        atrium.AddAttendee(participant);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }

    }

  }
}
