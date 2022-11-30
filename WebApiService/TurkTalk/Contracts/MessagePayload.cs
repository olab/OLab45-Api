using OLabWebAPI.Services.TurkTalk.Contracts;

namespace OLabWebAPI.TurkTalk.Contracts
{
  public class Envelope
  {
    public string To { get; set; }
    public Learner From { get; set;  }

    public Envelope()
    {

    }
  }

  public class MessagePayload
  {
    public Envelope Envelope { get; set; }
    public string Data { get; set; }

    public MessagePayload()
    {
    }
  }
}
