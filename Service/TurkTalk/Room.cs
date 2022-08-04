using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using OLabWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;

namespace TurkTalk.Contracts
{
  public class Room
  {
    public Participant moderator;
    public Dictionary<string /* connectionId */, Participant> Attendees
      = new Dictionary<string, Participant>();
    public readonly IHubContext<TurkTalkHub> hubContext;

    public string Name { get; }
    public Conference Conference { get; }
    public readonly ILogger logger;

    private Atrium _atrium;
    private bool isBotModerator = false;

    /// <summary>
    /// Create a room
    /// </summary>
    /// <param name="conference">Owning conference</param>
    /// <param name="name">Room name</param>
    public Room(Conference conference, string name)
    {
      Conference = conference ?? throw new ArgumentNullException(nameof(conference));
      logger = Conference.logger ?? throw new ArgumentNullException(nameof(Conference.logger));
      hubContext = conference.hubContext ?? throw new ArgumentNullException(nameof(conference.hubContext));

      if (string.IsNullOrEmpty(name))
        throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

      Name = name;

      _atrium = new Atrium(this);

      logger.LogInformation($"Created room '{Name}'.");
    }

    public static string GetNodeName(string name)
    {
      var parts = name.Split("|");
      if (parts.Count() != 2)
        throw new Exception($"{name} is not a valid atrium name");

      return parts[0];
    }

    public static string GetRoomName(string name)
    {
      var parts = name.Split("|");
      if (parts.Count() != 2)
        throw new Exception($"{name} is not a valid atrium name");

      return parts[1];
    }

    /// <summary>
    /// Tests if connection id within participants
    /// </summary>
    /// <param name="connectionId">Connection id to search for</param>
    /// <returns>true/false</returns>
    public bool ContainsConnectionId(string connectionId)
    {
      return GetAttendee(connectionId) != null;
    }

    /// <summary>
    /// Message received
    /// </summary>
    /// <param name="fromConnectionId">connection id that sent message</param>
    /// <param name="payload">Message payload</param>
    internal void MessageReceived(string fromConnectionId, MessagePayload payload)
    {
      var recipient = GetParticipant(payload.Envelope.ToConnectionId);
      if (recipient == null)
      {
        logger.LogError($"Recipient '{payload.Envelope.ToConnectionId[..3]}' cannot be found message");
        return;
      }

      //  echo message back to the sender
      var echoPayload = MessagePayload.GenerateEcho(payload);
      echoPayload.Envelope.ToConnectionId = fromConnectionId;
      Conference.SendMessageTo(fromConnectionId, "echo", JsonSerializer.Serialize(echoPayload));

      // send message to it's final destination
      payload.Envelope.ToConnectionId = recipient.ConnectionId;
      payload.Envelope.FromId = fromConnectionId;
      Conference.SendMessageTo(payload.Envelope.ToConnectionId, "message", JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Get participant in room
    /// </summary>
    /// <param name="key">Id of participant</param>
    /// <returns>Participant</returns>
    public Participant GetParticipant(string key = "")
    {
      var participant = GetModerator(key);
      if (participant == null)
        participant = GetAttendee(key);

      return participant;
    }

    /// <summary>
    /// Get moderator for room
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Participant GetModerator(string key = "")
    {
      // test if just returning current Moderator
      if (string.IsNullOrEmpty(key))
        return moderator;

      // test if have moderator assigned AND identifed by the key
      if ((moderator != null) && moderator.IsIdentifiedBy(key))
        return moderator;

      return null;
    }

    /// <summary>
    /// Get attendee in room
    /// </summary>
    /// <param name="participant">Participant to look for</param>
    /// <returns>Participant or null</returns>
    public Participant GetAttendee(Participant participant)
    {
      var attendee = Attendees.Values.FirstOrDefault(x => x.IsIdentifiedBy(participant));

      // if no assigned attendee, maybe it's unassigned (in atrium)
      if ((_atrium != null) && (attendee == null))
        attendee = _atrium.GetUnassignedAttendees().FirstOrDefault(x => x.IsIdentifiedBy(participant));

      return attendee;
    }

    /// <summary>
    /// Get attendee in room
    /// </summary>
    /// <param name="key">Name or ConnectionId to look for</param>
    /// <returns>Participant or null</returns>
    public Participant GetAttendee(string key)
    {
      var attendee = Attendees.Values.FirstOrDefault(x => x.IsIdentifiedBy(key));
      return attendee;
    }

    /// <summary>
    /// Get list of waiting-to-be-assigned (atrium) attendees
    /// </summary>
    /// <returns>List of participants</returns>
    internal IList<Participant> GetUnassignedAttendees()
    {
      if (_atrium != null)
        return _atrium.GetUnassignedAttendees();

      return new List<Participant>();
    }

    /// <summary>
    /// Add moderator to room
    /// </summary>
    /// <param name="connectionId">Requesting connection id</param>
    /// <param name="moderator">Moderator to add to room</param>
    /// <param name="isBot">Moderator is a bot</param>
    /// <returns>Newly added moderator</returns>
    internal Participant AddModerator(string connectionId, Participant moderator, bool isBot = false)
    {
      if (moderator is null)
        throw new ArgumentNullException(nameof(moderator));

      // if same moderator already in room, update
      // it with incoming modierator in case connection id changed
      if (GetModerator(moderator.Id) != null)
        logger.LogWarning($"Moderator {this.moderator.Name} already exists");

      // update connection id, in case it's changed
      moderator.ConnectionId = connectionId;

      this.moderator = moderator;
      this.moderator.RoomName = Name;
      this.moderator.IsAssigned = true;

      isBotModerator = isBot;

      // respond to moderator with status information
      Conference.SendConnectionStatus(connectionId, moderator);

      if (isBotModerator)
      {
        if (_atrium != null)
        {
          var unassignedAttendees = _atrium.GetUnassignedAttendees();

          // don't need atrium any more
          _atrium = null;

          foreach (var attendee in unassignedAttendees)
            AssignAttendee(connectionId, this.moderator, attendee);
        }
      }
      else
      {
        // send unassigned attendees list to moderators
        BroadcastUnassignedAttendeeList(connectionId);
      }

      logger.LogInformation($"Added moderator '{moderator}' to room '{Name}'");

      return moderator;
    }

    /// <summary>
    /// Add attendee to room
    /// </summary>
    /// <param name="connectionId">Requesting connection id</param>
    /// <param name="attendee">Attendee to add</param>
    /// <param name="removeIfExists">Flag to remove if already exists</param>
    /// <returns>attendee</returns>
    public Participant AddAttendee(string connectionId, Participant attendee, bool removeIfExists = true)
    {
      if ((GetAttendee(attendee) != null) && removeIfExists)
        Attendees.Remove(attendee.Id);

      // update connection id, in case it's changed
      attendee.ConnectionId = connectionId;

      // if have atrium, add attendee since moderator first 
      // needs to manually accept to room
      if (_atrium != null)
      {
        _atrium.AddAttendee(attendee);
        BroadcastUnassignedAttendeeList();
      }
      else
        Attendees.Add(attendee.Id, attendee);

      // respond to requestor with status information
      Conference.SendConnectionStatus(connectionId, attendee);

      return attendee;

    }

    /// <summary>
    /// Assign attendees to Room from atrium
    /// </summary>
    /// <param name="fromConnectionId">Connection id of requestor</param>
    /// <param name="moderator">Moderator making request</param>
    /// <param name="attendee">Participant to assign</param>
    public void AssignAttendee(string fromConnectionId, Participant moderator, Participant attendee)
    {
      if (moderator is null)
        throw new ArgumentNullException(nameof(moderator));

      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      if (_atrium is null)
        throw new Exception($"Room '{Name}' has no atrium to assign from");

      try
      {
        logger.LogInformation($"AssignAttendee: '{attendee}' to '{moderator}'");

        attendee.IsAssigned = true;
        attendee.RoomName = Name;

        // if have atrium, remove attendee since they should
        // be in a room now.
        if (_atrium != null)
          _atrium.RemoveAttendee(attendee.SessionId);

        var payload = new CommandAssignedPayload
        {
          Envelope = new Envelope
          {
            RoomName = moderator.RoomName,
            ToConnectionId = attendee.ConnectionId
          },
          Data = moderator
        };

        // add attendee to room
        Attendees.Add(attendee.Id, attendee);

        Conference.SendMessageTo(attendee.ConnectionId, "command", JsonSerializer.Serialize(payload));

        // update moderators of new unassigned list
        BroadcastUnassignedAttendeeList(fromConnectionId);

      }
      catch (Exception ex)
      {
        logger.LogError($"AssignAttendee exception: {ex.Message}");

      }
    }

    /// <summary>
    /// Send unassigned attendees list to moderator
    /// </summary>
    /// <param name="toConnectionId">connection id that receives the message</param>
    public void BroadcastUnassignedAttendeeList(string toConnectionId = null)
    {
      // skip room if no moderator assigned to send list to
      Participant moderator = GetModerator();
      if (moderator == null)
      {
        logger.LogDebug($"No moderator for room '{Name}' for unassigned attendee update");
        return;
      }

      if (_atrium == null)
      {
        logger.LogDebug($"Room '{Name}' has no atrium for unassigned attendee update");
        return;
      }

      // if no recipient connection Id, get last known 
      // connectionId for moderator
      if (string.IsNullOrEmpty(toConnectionId))
      {
        logger.LogDebug($"Using default moderator connection id");
        toConnectionId = moderator.ConnectionId;
      }

      // transform list for moderator - make attendees a destination, not a source
      // foreach (var attendee in _atrium.GetUnassignedAttendees())
      // {
      //   unassignedAttendees.Add(new Participant
      //   {
      //     Name = attendee.Name,
      //     SessionId = attendee.SessionId               
      //   });
      // }

      var unassignedAttendees = _atrium.GetUnassignedAttendees();
      if (_atrium.GetUnassignedAttendees().Count == 0)
        logger.LogDebug($"Room '{Name}' has {unassignedAttendees.Count} unassigned attendees");

      var payload = new CommandAttendeesPayload
      {
        Envelope = new Envelope(toConnectionId, moderator),
        Data = unassignedAttendees
      };

      logger.LogDebug($"Notifying '{moderator}' in room '{Name}' of unassigned list update");

      Conference.SendMessageTo(toConnectionId, "command", JsonSerializer.Serialize(payload));

    }

    /// <summary>
    /// Disconnect session 
    /// </summary>
    /// <param name="connectionId">SignalR connection id</param>
    /// <returns>true/false</returns>
    public bool DisconnectSession(string connectionId)
    {
      if (DisconnectAttendee(connectionId) || DisconnectModerator(connectionId))
        return true;

      return false;
    }

    /// <summary>
    /// Disconnect attendees from atrium/room
    /// </summary>
    /// <param name="connectionId">connection id to disconnect</param>
    /// <returns>true/false</returns>
    public bool DisconnectAttendee(string connectionId)
    {
      // test if unassigned attendee exists
      if (_atrium != null)
      {
        if (_atrium.RemoveAttendee(connectionId))
        {
          logger.LogDebug($"Removed attendee '{connectionId}' from '{Name}' unassigned list.");
          BroadcastUnassignedAttendeeList();
          return true;
        }
      }

      // get (hopefully) assigned attendee
      Participant attendee = GetAttendee(connectionId);
      if (attendee == null)
      {
        logger.LogError($"Attempted to disconnect attendees @ '{connectionId}' from '{Name}', but was not assigned");
        return false;
      }

      logger.LogDebug($"Removing attendee '{attendee.ConnectionId}' from '{Name}'.");
      Attendees.Remove(attendee.ConnectionId);

      // TODO: tell moderator that attendee has gone

      return true;
    }

    /// <summary>
    /// Disconnect moderator from room
    /// </summary>
    /// <param name="connectionId">connection id to disconnect</param>
    public bool DisconnectModerator(string connectionId)
    {
      var moderator = GetModerator(connectionId);
      if (moderator == null)
      {
        logger.LogError($"Attempted to disconnect moderator @ '{connectionId}', but was unknown");
        return false;
      }

      this.moderator = null;

      // if this room is for a bot, then we are done
      // since the conference will be responsible 
      // for deleting the room
      if (isBotModerator)
        return true;

      // if not existing atrium, then create one
      // so we can add any existing attendees to it
      if (_atrium == null)
        _atrium = new Atrium(this);

      // move all attendees to atrium
      foreach (var attendee in Attendees.Values)
      {
        _atrium.AddAttendee(attendee);
        // respond to participant with new status information
        Conference.SendConnectionStatus(attendee.ConnectionId, attendee);
      }

      return true;
    }

  }

}