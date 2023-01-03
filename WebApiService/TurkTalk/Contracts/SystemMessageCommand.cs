using OLabWebAPI.TurkTalk.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}