using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{

  public class MessagePayload : Payload
  {
    public string Data { get; set; }

    public static MessagePayload GenerateEcho(MessagePayload source)
    {
      return new MessagePayload
      {
        Envelope = source.Envelope.EchoEnvelope(),
        Data = source.Data
      };
    }
  }
}