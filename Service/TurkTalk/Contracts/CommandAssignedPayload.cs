using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandAssignedPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandAssignedPayload(Participant moderator, Participant attendee, Participant serverAttendee)
    {
      Command = "ASSIGNED";
      Envelope = new Envelope
      {
        FromId = moderator.SessionId,
        FromName = moderator.Name,
        ToName = attendee.PartnerName,
        ToId = attendee.PartnerSessionId,
        RoomName = moderator.RoomName
      };

      Data = serverAttendee;
    }
  }

}