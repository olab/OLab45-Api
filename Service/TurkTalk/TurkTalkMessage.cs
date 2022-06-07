using System;
using System.Collections.Generic;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  public class TurkTalkMessage
  {
    public int WindowIndex { get; set; }
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }

    public TurkTalkMessage(Participant info, string message )
    {
        ConnectionId = info.ConnectionId;
        Name = info.Name;
        Message = message;
    }  
  }

}