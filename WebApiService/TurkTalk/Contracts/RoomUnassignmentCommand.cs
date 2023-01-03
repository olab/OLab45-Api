using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Dawn;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a command to remove a connection from a room
  /// </summary>
  public class RoomUnassignmentCommand : CommandMethod
  {
    public Participant Data { get; set; }

    public RoomUnassignmentCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "learnerunassignment")
    {
      Guard.Argument(participant).NotNull(nameof(participant));
      Data = participant;
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}