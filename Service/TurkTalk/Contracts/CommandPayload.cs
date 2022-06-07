using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class CommandPayload : Payload
  {
    public string Command { get; set; }    
  }
}