using System;
using System.Collections.Generic;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  public class TurkTalkMessage
  {
    public int WindowIndex { get; set; }
    public string SessionId { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }

    public TurkTalkMessage(Participant info, string message )
    {
        SessionId = info.SessionId;
        Name = info.Name;
        Message = message;
    }  
  }

}