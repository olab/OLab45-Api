using Microsoft.AspNetCore.SignalR;
using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Learner : Participant
  {
    private const string _prefix = "learner";

    public Learner(string roomName, HubCallerContext context) : base(context)
    {
      var roomNameParts = roomName.Split("/");

      TopicName = roomNameParts[0];
      RoomName = TopicName;
      CommandChannel = $"{TopicName}/{_prefix}/{UserId}";

      // test if topic and room provided
      if (roomNameParts.Length == 2)
        AssignToRoom(Convert.ToInt32(roomNameParts[1]));
    }

    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base(topicName, userName, nickName, connectionId)
    {
    }
    public override void AssignToRoom(int index)
    {
      RoomName = $"{TopicName}/{index}";
    }
  }
}