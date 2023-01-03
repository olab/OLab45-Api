using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class RoomRejoinedCommand : CommandMethod
  {
    public Participant Data { get; set; }

    public RoomRejoinedCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "roomrejoined")
    {
      Data = participant;
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}