using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Participant
  {
    public string SessionId { get; set; }
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public bool InChat { get; set; }
    public string PartnerId { get; set; }
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
      ConnectionId = connectionId;
      LastReceived = DateTime.UtcNow;
      PartnerId = string.Empty;
    }

    public Participant(string name, string connectionId)
    {
      Name = name;
      ConnectionId = connectionId;
      LastReceived = DateTime.UtcNow;
      PartnerId = string.Empty;
    }

    public bool IsConnected { get { return !string.IsNullOrEmpty(ConnectionId); } }
    public override string ToString()
    {
      return $"'{Name} {SessionId} {InChat} {LastReceived}'";
      // return $"'{Name} {ConnectionId} {InSession} {LastReceived}'";
    }

    /// <summary>
    /// Disconnect client from session
    /// </summary>
    public void Disconnect()
    {
      InChat = false;
    }
  }
}