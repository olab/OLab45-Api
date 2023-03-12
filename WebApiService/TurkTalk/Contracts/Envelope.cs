
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.Common.Contracts
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
}
