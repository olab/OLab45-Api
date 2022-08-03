using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Participant
  {
    public string ConnectionId { get; set; }
    public string SessionId { get; set; }
    public string Name { get; set; }
    public bool IsAssigned { get; set; }
    public string PartnerId { get; set; }
    public string PartnerName { get; set; }
    public DateTime LastReceived { get; set; }
    public string RoomName { get; set; }

    public Participant()
    {
      LastReceived = DateTime.UtcNow;
      PartnerId = string.Empty;
      SessionId = Guid.NewGuid().ToString();
    }

    public Participant(string name)
    {
      Name = name;
      // ConnectionId = connectionId;
      LastReceived = DateTime.UtcNow;
      PartnerId = string.Empty;
    }

    public string Id { get { return SessionId; }}
    
    // public bool IsConnected { get { return !string.IsNullOrEmpty(ConnectionId); } }
    public override string ToString()
    {
      if ( !string.IsNullOrEmpty( SessionId ) && !string.IsNullOrEmpty( ConnectionId ))
        return $"{Name} {SessionId[..3]}:{ConnectionId[..3]}";

      if ( !string.IsNullOrEmpty( SessionId ) )
        return $"{Name} {SessionId[..3]}:???";

      return "???:???";
    }

    /// <summary>
    /// Disconnect client from session
    /// </summary>
    public void Disconnect()
    {
      IsAssigned = false;
    }

    /// <summary>
    /// Test if participant is identified by 'key'
    /// </summary>
    /// <param name="key">Connection id or name</param>
    /// <returns>true/false</returns>
    internal bool IsIdentifiedBy(Participant participant)
    {
      return (participant.ConnectionId == ConnectionId) ||
              (participant.Name == Name) ||
              (participant.SessionId == SessionId);
    }

    /// <summary>
    /// Test if participant is identified by 'key'
    /// </summary>
    /// <param name="key">Connection id or name</param>
    /// <returns>true/false</returns>
    internal bool IsIdentifiedBy(string key)
    {
      return (key == ConnectionId) || (key == Name) || ( key == SessionId );
    }
  }
}