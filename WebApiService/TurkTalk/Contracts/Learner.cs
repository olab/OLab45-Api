using Microsoft.AspNetCore.SignalR;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using OLabWebAPI.TurkTalk.Contracts;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class Learner : Participant
  {
    public const string Prefix = "learner";
    public RegisterAttendeePayload Session{ get; set; }

    public Learner()
    {
    }

    public Learner(RegisterAttendeePayload session, HubCallerContext context) : base(context)
    {
      Session = session;
      string[] roomNameParts = session.RoomName.Split("/");

      TopicName = roomNameParts[0];
      RoomName = TopicName;
      CommandChannel = $"{TopicName}/{Prefix}/{UserId}";

      // test if topic and room provided
      if (roomNameParts.Length == 2)
        AssignToRoom(Convert.ToInt32(roomNameParts[1]));
    }

    public Learner(Participant participant)
      : base(participant.TopicName, participant.UserId, participant.NickName, participant.ConnectionId)
    {
      CommandChannel = participant.CommandChannel;
    }

    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base(topicName, userName, nickName, connectionId)
    {
    }

    public static string MakeCommandChannel(Participant source)
    {
      return $"{source.TopicName}/{Learner.Prefix}/{source.UserId}";
    }

    public override void AssignToRoom(int index)
    {
      RoomNumber = index;
      RoomName = $"{TopicName}/{index}";
    }
  }
}