using System;
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
    /// Remove assigned learner from atrium
    /// </summary>
    /// <param name="learner">Learner to remove</param>
    /// <param name="topicName">Topic id</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public void Message(MessagePayload payload)
    {
      try
      {
        _logger.LogInformation($"Message: from '{payload.Envelope.From}', {payload.Data}");

        // get or create a topic
        var topic = _conference.GetCreateTopic(payload.Envelope.From.TopicName, false);
        if (topic == null)
          return;

        // dispatch message
        topic.Conference.SendMessage(
          new MessageCommand(
            payload));
      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendeeASync exception: {ex.Message}");
      }
    }
  }
}
