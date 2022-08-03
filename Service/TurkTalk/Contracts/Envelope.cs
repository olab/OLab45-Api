using OLabWebAPI.Common;

namespace TurkTalk.Contracts
{
  public class Envelope
  {
    public string RoomName { get; set; }
    public string FromId { get; set; }
    public string FromName { get; set; }
    public string ToConnectionId { get; set; }
    public string ToName { get; set; }

    public Envelope()
    {

    }

    public Envelope(string connectionId, Participant participant)
    {
      ToName = participant.Name;
      ToConnectionId = connectionId;
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
        ToConnectionId = source.ToConnectionId,
        ToName = source.ToName,
        RoomName = source.RoomName
      };
    }

    public Envelope EchoEnvelope()
    {
      Envelope temp = CopyFrom(this);
      temp.ToConnectionId = FromId;
      temp.ToName = FromName;
      temp.FromId = ToConnectionId;
      temp.FromName = ToName;
      return temp;
    }
  }
}