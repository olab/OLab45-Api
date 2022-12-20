using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a command to remove a connection from a room
  /// </summary>
  public class RoomUnassignmentCommand : CommandMethod
  {
    public Participant Data { get; set; }

    public RoomUnassignmentCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "roomunassignment")
    {
      Data = participant;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}