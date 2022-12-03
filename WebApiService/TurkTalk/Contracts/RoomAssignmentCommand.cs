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
    public Participant Data { get; set; }

    public RoomAssignmentCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "roomassignment")
    {
      Data = participant;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}