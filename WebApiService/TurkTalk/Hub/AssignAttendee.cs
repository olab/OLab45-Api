using System;
using System.Threading.Tasks;
using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;
using OLabWebAPI.TurkTalk.Contracts;

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
    public async Task AssignAttendee(Learner learner, string topicName)
    {
      try
      {
        Guard.Argument(topicName).NotNull(nameof(topicName));
        _logger.LogInformation(
          $"AssignAttendeeASync: '{learner.CommandChannel}', {topicName} ({ConnectionId.Shorten(Context.ConnectionId)})");

        var topic = _conference.GetCreateTopic(learner.TopicName, false);
        if (topic == null)
          return;

        topic.RemoveFromAtrium(learner);

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
        _logger.LogError($"AssignAttendeeASync exception: {ex.Message}");
      }
    }
  }
}
