using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandUnassignedPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandUnassignedPayload()
    {
      Command = "UNASSIGNED";
    }
  }

}