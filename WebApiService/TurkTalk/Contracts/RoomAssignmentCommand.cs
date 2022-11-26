using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class RoomAssignmentCommand : CommandMethod
  {
    public string Data { get; set; }

    public RoomAssignmentCommand(string recipientGroupName, Participant group) : base(recipientGroupName, "roomassignment")
    {
      Data = group.TopicName;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}