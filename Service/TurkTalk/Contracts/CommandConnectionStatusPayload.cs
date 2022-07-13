using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandConnectionStatusPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandConnectionStatusPayload(string senderConnectionId, Participant participant)
    {
      Envelope = new Envelope(participant);
      Data = participant;
      Command = "CONNECTSTATUS";
      Envelope.ToId = senderConnectionId;
    }
  }

}