using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class ModeratorGroupName : GroupName
  {
    public ModeratorGroupName(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base("moderator", topicName, userName, nickName, connectionId)
    {
    }
  }
}
