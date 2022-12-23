using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class RoomAssignmentData
  {
    public Participant Local{ get; set; }
    public Participant Remote { get; set; }

  }

  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class RoomAssignmentCommand : CommandMethod
  {
    public RoomAssignmentData Data { get; set; }

    public RoomAssignmentCommand(Participant participant, Participant moderator = null) : base(participant.CommandChannel, "roomassignment")
    {
      Data = new RoomAssignmentData { Local = participant, Remote = moderator };
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}