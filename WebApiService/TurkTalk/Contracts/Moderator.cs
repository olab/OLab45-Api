using Microsoft.AspNetCore.SignalR;
using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Moderator : Participant
  {
    public Moderator(string topicName, HubCallerContext context) : base( topicName, context )
    {
    }

    public Moderator(string topicName, string userName = null, string nickName = null, string connectionId = null)
  : base("moderator", topicName, userName, nickName, connectionId)
    {
    }
  }
}
