using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using OLabWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using System;

namespace TurkTalk.Contracts
{
  public class Room
  {
    public Participant Moderator;
    public Dictionary<string /* connectionId */, Participant> Attendees
      = new Dictionary<string, Participant>();
    public readonly IHubContext<TurkTalkHub> HubContext;   

    public string Name { get; }
    public Atrium Atrium { get; }
    public readonly ILogger Logger;

    public Room(Atrium atrium, string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
      Atrium = atrium ?? throw new ArgumentNullException(nameof(atrium));
      Logger = Atrium.Logger ?? throw new ArgumentNullException(nameof(Atrium.Logger));
      HubContext = atrium.HubContext ?? throw new ArgumentNullException(nameof(atrium.HubContext));      

      Name = name;
      Logger.LogInformation($"Created room '{Name}'");
    }
    
    /// <summary>
    /// Tests if connection id within attendees
    /// </summary>
    /// <param name="sessionId">Connection id to search for</param>
    /// <returns>true/false</returns>
    public bool ContainsSessionId(string sessionId)
    {
      if (Moderator.SessionId == sessionId)
        return true;

      if (Attendees.ContainsKey(sessionId))
        return true;

      return false;
    }

    public Participant GetModerator(string key = "")
    {
      // test if just returning current Moderator
      if (string.IsNullOrEmpty(key))
        return Moderator;

      // test if have moderator assigned AND identifed by the key
      if ((Moderator != null) && ((Moderator.SessionId == key) || (Moderator.Name == key) || (Moderator.SessionId == key)))
        return Moderator;

      return null;
    }

    /// <summary>
    /// Get attendees from attendees
    /// </summary>
    /// <param name="key">Name or SessionId to look for</param>
    /// <returns>Participant or null</returns>
    public Participant GetAttendee(string key)
    {
      var attendees = Attendees.Values.FirstOrDefault(x => x.SessionId == key || x.Name == key);
      return attendees;
    }

    public void SetModerator(Participant moderator)
    {
      Moderator = moderator;
    }

    public bool AddAttendee(Participant attendees, bool removeIfExists)
    {
      if (GetAttendee(attendees.SessionId) != null)
      {
        if (removeIfExists)
        {
          Attendees.Remove(attendees.SessionId);
          // Groups.RemoveFromGroupAsync( attendees.ConnectionId, Name );
        }
        else
          return false;
      }

      Attendees.Add(attendees.SessionId, attendees);
      return true;

    }

  }

}