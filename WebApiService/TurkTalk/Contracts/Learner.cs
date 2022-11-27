using Microsoft.AspNetCore.SignalR;
using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Learner : Participant
  {
    public Learner(string topicName, HubCallerContext context) : base(topicName, context)
    {
    }

    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base("learner", topicName, userName, nickName, connectionId)
    {
    }
  }
}