using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;

namespace OLabWebAPI.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Register moderator to atrium
    /// </summary>
    /// <param name="topicName">Topic id</param>
    /// <param name="isRejoining">Is rejoining a previously attended topic</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task RegisterAttendee(string topicName)
    {
      try
      {
        var identity = (ClaimsIdentity)Context.User.Identity;
        var nickName = identity.FindFirst("name").Value;
        var userId = identity.FindFirst(ClaimTypes.Name).Value;

        Guard.Argument(userId).NotEmpty(userId);
        Guard.Argument(topicName).NotEmpty(topicName);

        var learner = new LearnerGroupName(topicName, userId, nickName, Context.ConnectionId );
        _logger.LogInformation($"RegisterAttendee: '{learner.GroupName}' id: {Context.ConnectionId}");

        // get or create a topic
        var topic = _conference.GetCreateTopic(learner.TopicName);        
        await topic.AddLearnerToAtriumAsync(learner, Context.ConnectionId);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }
    }
  }
}
