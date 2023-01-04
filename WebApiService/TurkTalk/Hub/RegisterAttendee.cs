using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Register attendee to room
    /// </summary>
    /// <param name="roomName">Room name</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task RegisterAttendee(string roomName)
    {
      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        Learner learner = new Learner(roomName, Context);
        _logger.LogDebug($"RegisterAttendee: room: {roomName} '{learner.CommandChannel} ({ConnectionId.Shorten(Context.ConnectionId)})");

        // get or create a conference topic
        Venue.Topic topic = _conference.GetCreateTopic(learner.TopicName);
        var room = topic.GetParticipantRoom(learner);

        // if no existing room contains learner, add learner to 
        // topic atrium
        if (room == null)
        {
          _logger.LogDebug($"RegisterAttendee: adding to '{roomName}' atrium");
          await topic.AddToAtriumAsync(learner);
        }

        // user already 'known' to an existing room
        else
        {

          // if room has no moderator (i.e. moderator may have
          // disconnected) add the attendee to the topic atrium
          if (room.Moderator == null)
          {
            _logger.LogDebug($"RegisterAttendee: room '{roomName}' has no moderator.  Assigning to atrium.");
            await topic.AddToAtriumAsync(learner);
          }

          // user already 'known' to a room AND room is moderated, so
          // signal room assignment to re-attach the learner to the room
          else
          {
            _logger.LogInformation($"RegisterAttendee: assigning participant to existing room '{roomName}'");
            await AssignAttendee(learner, room.Name);
          }
        }

      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }
    }
  }
}
