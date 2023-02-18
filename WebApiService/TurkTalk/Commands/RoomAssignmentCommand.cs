﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
  public class RoomAssignmentPayload
  {
    public Learner Local { get; set; }
    public Moderator Remote { get; set; }
    public int SlotIndex { get; set; }
  }

  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class RoomAssignmentCommand : CommandMethod
  {
    public RoomAssignmentPayload Data { get; set; }

    public RoomAssignmentCommand(Learner local, Moderator remote = null, int slotIndex = 0 ) :
          base(local == null ? remote.CommandChannel : local.CommandChannel, "roomassignment")
    {
      Data = new RoomAssignmentPayload { Local = local, Remote = remote, SlotIndex = slotIndex };
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JToken.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}