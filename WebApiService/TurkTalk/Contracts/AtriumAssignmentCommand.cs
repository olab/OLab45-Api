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

  }
}