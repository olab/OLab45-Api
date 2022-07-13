using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  // public class MessagePayload
  // {
  //   public Participant SenderInfo { get; set; }
  //   public string Message { get; set; }
  // }

  public class CommandAttendeesPayload : CommandPayload
  {
    public IEnumerable<Participant> Data { get; set; }

    public CommandAttendeesPayload(Participant moderator, IEnumerable<Participant> data )
    {
      Envelope = new Envelope(moderator);
      Command = "ATTENDEES";
      Data = data;
    }
  }

}