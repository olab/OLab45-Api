using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;

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
    public void AssignAttendee(Learner learner, string topicName)
    {
      try
      {
        Guard.Argument(topicName).NotNull(nameof(topicName));
        _logger.LogInformation($"AssignAttendee: '{learner}', {topicName}");

        var topic = _conference.GetCreateTopic(learner.TopicName, false);
        if (topic == null)
          return;

        topic.RemoveFromAtrium(learner);

      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendee exception: {ex.Message}");
      }
    }
  }
}
