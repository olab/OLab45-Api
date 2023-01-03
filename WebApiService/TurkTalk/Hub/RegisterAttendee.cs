using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
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
        _logger.LogInformation($"RegisterAttendee: room: {roomName} '{learner.CommandChannel} ({ConnectionId.Shorten(Context.ConnectionId)})");

        // test if participant was not already assigned to room (i.e. rejoin)
        if (!learner.IsAssignedToRoom())
        {
          _logger.LogInformation($"RegisterAttendee: adding to '{roomName}' atrium");

          // get or create a conference topic
          Venue.Topic topic = _conference.GetCreateTopic(learner.TopicName);

          // if learner not already in a topic room, add to 
          // topic atrium, else assign to room as in
          var room = topic.GetParticipantRoom(learner);

          if (room == null)
            await topic.AddToAtriumAsync(learner);
          else
          {
            _logger.LogInformation($"RegisterAttendee: found participant in room '{roomName}'");
            await AssignAttendee(learner, room.Name);
          }

        }
        else
        {
          _logger.LogInformation($"RegisterAttendee: rejoining room '{roomName}'");
          await AssignAttendee(learner, roomName);
        }

      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }
    }
  }
}
