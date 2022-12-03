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
    /// Moderator assigns a learner (remove from atrium)
    /// </summary>
    /// <param name="learner">Learner to assign</param>
    /// <param name="topicName">Topic id</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task AssignAttendee(Learner learner, string roomName)
    {
      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        _logger.LogInformation(
          $"AssignAttendeeAsync: '{learner.CommandChannel}', {roomName} ({ConnectionId.Shorten(Context.ConnectionId)})");

        var topic = _conference.GetCreateTopic(learner.TopicName, false);
        if (topic == null)
          return;

        topic.RemoveFromAtrium(learner);

        var room = topic.GetRoom(roomName);
        if (room != null)
          await room.AddLearnerAsync(learner, learner.ConnectionId);

        // add the moderator connection id to the newly
        // assigned learner's command group name
        await topic.Conference.AddConnectionToGroupAsync(
          learner.CommandChannel,
          Context.ConnectionId);

        // post a message to the learner that they've
        // been assigned to a room
        topic.Conference.SendMessage(
          new RoomAssignmentCommand(
            learner.CommandChannel,
            learner));

      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendeeAsync exception: {ex.Message}");
      }
    }
  }
}
