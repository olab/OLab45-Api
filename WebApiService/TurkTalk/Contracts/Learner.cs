using Microsoft.AspNetCore.SignalR;
using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Learner : Participant
  {
    private const string _prefix = "learner";

    public string RoomGroupName { get; private set; }

    public Learner(HubCallerContext context) : base(context)
    {
      Initialize();
    }

    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base(topicName, userName, nickName, connectionId)
    {
      Initialize();
    }

    private void Initialize()
    {
      if (RoomNumber.HasValue)
        RoomGroupName = $"{TopicName}/{RoomNumber.Value}/{UserId}";
      else
        RoomGroupName = null;
    }

    public override void AssignToRoom(int index)
    {
      RoomNumber = index;
      Initialize();
    }

    public override string MessageBox()
    {
      if (RoomNumber.HasValue)
        return $"{TopicName}/{RoomNumber.Value}/{_prefix}/{UserId}";
      return $"{TopicName}//{_prefix}/{UserId}";
    }
  }
}