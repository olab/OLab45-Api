using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandReassignedPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandReassignedPayload()
    {
      Command = "REASSIGNED";
    }
  }

}