using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class AtriumAssignmentCommand : CommandMethod
  {
    public Learner Data { get; set; }

    public AtriumAssignmentCommand(Participant participant, Learner atriumParticipant)
      : base(participant.CommandChannel, "atriumassignment")
    {
      Data = atriumParticipant;
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}