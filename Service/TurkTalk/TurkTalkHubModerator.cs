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
    /// <param name="atriumName">Atrium name</param>
    public void RegisterModerator(string moderatorName, string atriumName)
    {
      try
      {
        _logger.LogInformation($"RegisterModerator: '{moderatorName}', atrium {atriumName}");

        var participant = new Participant(moderatorName, Context.ConnectionId);
        var atrium = Conference.GetAtriumByName(atriumName, true);
        var attendees = atrium.AddModerator(participant);
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

        var atrium = Conference.GetAtriumByName(atriumName);

        // get requesting moderator based on connectionId
        var moderator = atrium.GetModerator( Context.ConnectionId );
        if ( moderator == null )
          throw new Exception($"AssignAttendee: moderator '{Context.ConnectionId}' in atrium '{atrium}' does not exist");

        // assign attendee to moderator's room
        atrium.AssignAttendee( moderator, attendee );        
      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendee exception: {ex.Message}");
      }

    }
  }
}
