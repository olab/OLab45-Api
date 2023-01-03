using OLabWebAPI.Services.TurkTalk.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.TurkTalk.Contracts
{
  public class Envelope
  {
    public string To { get; set; }
    public Learner From { get; set; }

    public Envelope()
    {
      From = new Learner();
    }
  }

  public class MessagePayload
  {
    public Envelope Envelope { get; set; }
    public string Data { get; set; }
    public string SessionId { get; set; }

    public MessagePayload()
    {
      Envelope = new Envelope();
    }

    /// <summary>
    /// Construct message to specific participant
    /// </summary>
    /// <param name="participant">Recipient</param>
    /// <param name="message">Message to send</param>
    public MessagePayload(Participant participant, string message)
    {
      Envelope = new Envelope();
      Envelope.To = participant.CommandChannel;
      Data = message;
    }
  }
}
