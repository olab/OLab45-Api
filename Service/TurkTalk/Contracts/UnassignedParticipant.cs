using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class UnassignedParticipant : Participant
  {
    public UnassignedParticipant(Room room, Participant moderator, Participant attendee)
    {
      Name = moderator.Name;
      PartnerSessionId = attendee.SessionId;
      PartnerName = attendee.Name;
      RoomName = room.Name;
      SessionId = moderator.SessionId;
    }

  }

}