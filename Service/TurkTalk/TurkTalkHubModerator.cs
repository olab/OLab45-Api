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
    /// <param name="name">Moderator's name</param>
    /// <param name="atriumName">Atrium name</param>
    /// <param name="sessionId">Optional id from previous assigned session</param>
    public void RegisterModerator(string name, string atriumName, string sessionId = null)
    {
      try
      {
        if (string.IsNullOrEmpty(name))
          throw new ArgumentNullException(nameof(name));

        if (string.IsNullOrEmpty(atriumName))
          throw new ArgumentNullException(nameof(atriumName));

        _logger.LogInformation($"RegisterModerator: '{name}', atrium '{atriumName}', sessionId '{sessionId}'");

        var atrium = Conference.GetAtriumByName(atriumName, true);

        // see if moderator was already known (in a room)
        var moderator = atrium.GetModerator(sessionId);

        // if moderator is new, generate Participant and add.  otherwise
        // update the connectionId for attendee.
        if (moderator == null)
        {
          moderator = new Participant(name);

          moderator.SetConnectionId( Context.ConnectionId );

          // if participant already had sessionId update it
          if (!string.IsNullOrEmpty(sessionId))
            moderator.SessionId = sessionId; 
                
          atrium.AddModerator(Context.ConnectionId, moderator);
          atrium.DumpAtrium();
        }
        else
          atrium.UpdateModerator(Context.ConnectionId, moderator);

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
