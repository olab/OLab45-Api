namespace OLabWebAPI.TurkTalk.Contracts
{
  public class RegisterAttendeePayload
  {
    public string ContextId { get; set; } 
    public uint MapId { get; set; } 
    public uint NodeId { get; set; } 
    public uint QuestionId { get; set; }

    public string RoomName { get; set; }
  }
}
