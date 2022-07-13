using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class Envelope
  {
    public string RoomName { get; set; }
    public string FromId { get; set; }
    public string FromName { get; set; }
    public string ToId { get; set; }
    public string ToName { get; set; }

    public Envelope()
    {

    }

    public Envelope(Participant participant)
    {
      ToName = participant.Name;
      ToId = participant.GetConnectionId();      
    }

    public Envelope(string name, string id)
    {
      FromName = name;
      FromId = id;
    }

    private static Envelope CopyFrom(Envelope source)
    {
      return new Envelope
      {
        FromId = source.FromId,
        FromName = source.FromName,
        ToId = source.ToId,
        ToName = source.ToName,
        RoomName = source.RoomName
      };
    }

    public Envelope EchoEnvelope()
    {
      Envelope temp = CopyFrom(this);
      temp.ToId = FromId;
      temp.ToName = FromName;
      temp.FromId = ToId;
      temp.FromName = ToName;
      return temp;
    }
  }
}