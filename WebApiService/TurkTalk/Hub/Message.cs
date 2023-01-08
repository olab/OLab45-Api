using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Data;
using OLabWebAPI.Endpoints;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.Contracts;
using System;

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
        _logger.LogInformation($"Message: from '{payload.Envelope.From}', {payload.Data} ({ConnectionId.Shorten(Context.ConnectionId)})");

        // get or create a topic
        Venue.Topic topic = _conference.GetCreateTopic(payload.Envelope.From.TopicName, false);
        if (topic == null)
          return;

        // dispatch message
        topic.Conference.SendMessage(
          new MessageMethod(payload));

        var userContext = GetUserContext();
        userContext.Session.SetSessionId(payload.Session.ContextId);

        // add message event session activity
        userContext.Session.OnQuestionResponse(
          payload.Session.MapId,
          payload.Session.NodeId,
          payload.Session.QuestionId,
          payload.Data);

      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendeeASync exception: {ex.Message}");
      }
    }
  }
}
