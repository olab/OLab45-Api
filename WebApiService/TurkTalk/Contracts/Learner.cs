using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Learner : Participant
  {
    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base("learner", topicName, userName, nickName, connectionId)
    {
    }
  }
}