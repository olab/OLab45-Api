using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Payload
  {
    public Envelope Envelope { get; set; }

    public string GetToId() { return Envelope.ToConnectionId; }
    public string GetFromId() { return Envelope.FromId; }
  }
}