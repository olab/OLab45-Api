using OLabWebAPI.TurkTalk.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class JumpNodeMethod : CommandMethod
  {
    public TargetNode Data { get; set; }
    public string SessionId { get; set; }
    public string From { get; set; }

    // message for specific group
    public JumpNodeMethod(JumpNodePayload payload) : base(payload.Envelope.To, "jumpnode")
    {
      Data = payload.Data;
      SessionId = payload.Session.ContextId;
      From = payload.Envelope.From.UserId;
    }
    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}