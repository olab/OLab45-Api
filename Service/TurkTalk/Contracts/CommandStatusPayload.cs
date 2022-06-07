using System.Collections.Generic;
using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class CommandStatusPayload : CommandPayload
  {
    public Participant Data { get; set; }

    public CommandStatusPayload()
    {
      Command = "CONNECTSTATUS";
    }
  }

}