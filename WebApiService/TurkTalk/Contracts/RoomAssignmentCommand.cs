using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class RoomAssignmentPayload
  {
    public Participant Local{ get; set; }
    public Participant Remote { get; set; }

  }

  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class RoomAssignmentCommand : CommandMethod
  {
    public RoomAssignmentPayload Data { get; set; }

    public RoomAssignmentCommand(Participant local, Participant remote = null) : base(local.CommandChannel, "roomassignment")
    {
      Data = new RoomAssignmentPayload { Local = local, Remote = remote };
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}