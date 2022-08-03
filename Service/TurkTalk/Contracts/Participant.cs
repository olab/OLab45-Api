using System;
using System.Collections.Generic;

namespace TurkTalk.Contracts
{
  public class Participant
  {
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public bool IsAssigned { get; set; }
    public string PartnerId { get; set; }
    public string PartnerName { get; set; }
    public DateTime LastReceived { get; set; } 
    public string RoomName { get; set; }

    public Participant()
    {
      
    }

    public Participant(string name, string connectionId = "")
    {
      Name = name;
      ConnectionId = connectionId;
      LastReceived = DateTime.UtcNow;
      PartnerId = string.Empty;
    }

    public bool IsConnected { get { return !string.IsNullOrEmpty(ConnectionId); } }
    public override string ToString()
    {
      return $"'{Name} {ConnectionId} {IsAssigned} {LastReceived}'";
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
    internal bool IsIdentifiedBy(string key)
    {
      return ( ( key == ConnectionId ) || ( key == Name ) );
    }
  }
}