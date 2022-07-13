using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Payload
  {
    public Envelope Envelope { get; set; }

    public string GetToId() { return Envelope.ToId; }
    public void SetToId( string connectionId) { Envelope.ToId = connectionId; }
    public string GetFromId() { return Envelope.FromId; }
  }
}