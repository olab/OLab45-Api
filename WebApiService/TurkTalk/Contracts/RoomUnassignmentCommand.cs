using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a command to remove a connection from a room
  /// </summary>
  public class RoomUnassignmentCommand : CommandMethod
  {
    public string Data { get; set; }

    public RoomUnassignmentCommand(string recipientGroupName, string connectionId) : base(recipientGroupName, "roomunassignment")
    {
      Data = connectionId;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}