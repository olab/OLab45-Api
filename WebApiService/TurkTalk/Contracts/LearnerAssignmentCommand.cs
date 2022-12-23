using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class LearnerAssignmentCommand : CommandMethod
  {
    public Participant Data { get; set; }

    public LearnerAssignmentCommand(Participant moderator, Participant learner) : base(moderator.CommandChannel, "learnerassignment")
    {
      Data = learner;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}