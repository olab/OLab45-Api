using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Participant
  {
    private string _partnerSessionId;
    private string _connectionId;

    public string SessionId { get; set; }
    public string Name { get; set; }
    public bool InChat { get; set; }

    public string PartnerSessionId
    {
      get { return _partnerSessionId; }
      set { _partnerSessionId = value; InChat = !string.IsNullOrEmpty(value); }
    }
    public string PartnerName { get; set; }
    public DateTime LastReceived { get; set; }
    public string RoomName { get; set; }

    public Participant()
    {

    }

    public Participant(string name, string connectionId, string sessionId)
    {
      SessionId = sessionId;
      Name = name;
      LastReceived = DateTime.UtcNow;
      PartnerSessionId = string.Empty;
    }

    public Participant(string name)
    {
      Name = name;
      LastReceived = DateTime.UtcNow;
      PartnerSessionId = string.Empty;
      SessionId = Guid.NewGuid().ToString();
    }

    public Participant(string name, string connectionId)
    {
      Name = name;
      LastReceived = DateTime.UtcNow;
      PartnerSessionId = string.Empty;
      SessionId = Guid.NewGuid().ToString();
    }

    public override string ToString()
    {
      return $"'{Name} SId:{SessionId.Substring(0, 3)} CId:{_connectionId.Substring(0, 3)} {InChat}'";
      // return $"'{Name} {ConnectionId} {InSession} {LastReceived}'";
    }

    /// <summary>
    /// Disconnect client from session
    /// </summary>
    public void Disconnect()
    {
      PartnerSessionId = null;
    }

    internal void SetConnectionId(string connectionId)
    {
      _connectionId = connectionId;
    }

    internal string GetConnectionId()
    {
      return _connectionId;
    }
  }
}