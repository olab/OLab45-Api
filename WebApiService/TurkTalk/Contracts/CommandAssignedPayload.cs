using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandAssignedPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandAssignedPayload()
    {
      Command = "ASSIGNED";
    }
  }

}