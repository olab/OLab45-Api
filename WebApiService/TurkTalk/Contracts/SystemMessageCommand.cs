using OLabWebAPI.TurkTalk.Contracts;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class SystemMessageCommand : Method
  {
    public string Data { get; set; }
    /// <summary>
    /// Defines a Moderator removed command method
    /// </summary>
    public SystemMessageCommand(MessagePayload payload) : base(payload.Envelope.To, "systemmessage")
    {
      Data = payload.Data;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}