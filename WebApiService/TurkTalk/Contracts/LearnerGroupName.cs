using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class LearnerGroupName : GroupName
  {
    public LearnerGroupName(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base("learner", topicName, userName, nickName, connectionId)
    {
    }
  }
}