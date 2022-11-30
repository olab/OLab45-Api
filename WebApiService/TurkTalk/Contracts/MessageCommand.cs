using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Services.TurkTalk.Venue;
using OLabWebAPI.TurkTalk.Contracts;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class MessageCommand : Method
  {
    public string Data { get; set; }

    // message for specific group
    public MessageCommand(MessagePayload payload) : base(payload.Envelope.From.CommandChannel, "message")
    {
      Data = payload.Data;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}