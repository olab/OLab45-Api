using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
  public class LearnerAssignmentPayload
  {
    public Participant Learner { get; set; }
    public int SlotIndex { get; set; }
  }

  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class LearnerAssignmentCommand : CommandMethod
  {
    public LearnerAssignmentPayload Data { get; set; }

    public LearnerAssignmentCommand(
      Participant moderator,
      Participant learner,
      int slotIndex) : base(moderator.CommandChannel, "learnerassignment")
    {
      Data = new LearnerAssignmentPayload { Learner = learner, SlotIndex = slotIndex };
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JToken.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}