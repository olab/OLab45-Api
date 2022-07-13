using TurkTalk.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OLabWebAPI.Services;
using Microsoft.AspNetCore.SignalR;

namespace OLabWebAPI.Services
{
  public class Atrium
  {
    public string Name { get; set; }
    public Conference Conference { get; internal set; }
    public readonly ILogger Logger;
    public readonly IHubContext<TurkTalkHub> HubContext;

    public IList<Room> Rooms = new List<Room>();
    public IList<Participant> attendees = new List<Participant>();

    public Atrium(Conference conference, string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException($"Atrium: '{nameof(name)}' cannot be null or empty.", nameof(name));

      Conference = conference ?? throw new ArgumentNullException(nameof(conference));
      Logger = conference.Logger ?? throw new ArgumentNullException(nameof(conference.Logger));
      HubContext = conference.HubContext ?? throw new ArgumentNullException(nameof(conference.HubContext));

      Name = name;
      Logger.LogInformation($"Created atrium '{Name}'");
    }

    internal void Open()
    {
      throw new NotImplementedException();
    }

    internal void Close()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get moderator from any Room of a atrium
    /// </summary>
    /// <param name="key">Name, ConnectionId, SessionId to look for</param>
    /// <returns>Participant</returns>
    public Participant GetModerator(string key)
    {
      Participant moderator = null;

      if ( string.IsNullOrEmpty( key ))
        return null;

      foreach (var Room in Rooms)
        moderator = Room.GetModerator(key);

      return moderator;
    }

    /// <summary>
    /// Get attendee from (any) atrium Room
    /// </summary>
    /// <param name="key">Name or sessionId to look for</param>
    /// <returns>Participant</returns>
    public Participant GetAttendee(string key)
    {
      Participant attendee = null;

      if ( string.IsNullOrEmpty( key ))
        return null;

      // look for participant in atrium first
      attendee = attendees.FirstOrDefault(x => x.SessionId == key);

      // if not found attendee, try the rooms
      if (attendee == null)
      {
        foreach (var Room in Rooms)
        {
          attendee = Room.GetAttendee(key);
          if (attendee != null)
            break;
        }
      }

      return attendee;
    }

    /// <summary>
    /// Get first room that is not manned by a moderator
    /// </summary>
    /// <param name="createIfNone">Optional param to create new Room</param>
    /// <returns>Attendees Room</returns>
    private Room GetUnmoderatedRoom(bool createIfNone = false)
    {
      var Room = Rooms.FirstOrDefault(x => x.GetModerator() == null);
      if ((Room == null) && createIfNone)
      {
        // create new Room, with index based on atrium name
        Room = new Room(this, $"{Name}|{Rooms.Count}");
        Rooms.Add(Room);
      }

      return Room;
    }

    /// <summary>
    /// Get Room containing participant
    /// </summary>
    /// <param name="participant">Participant</param>
    /// <returns>Attendees Room or null</returns>
    public Room GetRoomContainingParticipant(Participant participant)
    {
      if (participant is null)
        throw new ArgumentNullException(nameof(participant));

      foreach (var Room in Rooms)
      {
        if ((Room.GetAttendee(participant.ConnectionId) != null) ||
             (Room.GetModerator(participant.ConnectionId) != null))
          return Room;
      }

      return null;
    }

    /// <summary>
    /// Add moderator to atrium
    /// </summary>
    /// <param name="hub">SignalR Hub</param>
    /// <param name="moderator">Moderator to add to atrium</param>
    /// <returns>Newly added moderator</returns>
    internal Participant AddModerator(Participant moderator)
    {
      if (moderator is null)
        throw new ArgumentNullException(nameof(moderator));

      var penModerator = GetModerator(moderator.ConnectionId);
      if (penModerator != null)
      {
        Logger.LogWarning($"Moderator {penModerator.Name} already connected");
        return penModerator;
      }

      moderator.InChat = true;
      moderator.SessionId = Guid.NewGuid().ToString();

      var Room = GetUnmoderatedRoom(true);
      Room.SetModerator(moderator);

      Logger.LogInformation($"Adding moderator {moderator} to atrium '{Room.Name}'");

      moderator.RoomName = Room.Name;

      // respond to moderator with status information
      SendConnectionStatus(moderator);

      // send unassigned attendees list to moderators
      BroadcastUnassignedAttendeeList();

      return moderator;
    }

    /// <summary>
    /// Gets a room in the atrium by name
    /// </summary>
    /// <param name="roomName">Room name</param>
    /// <returns>Room or nll, if not found</returns>
    internal Room GetRoomByName(string roomName)
    {
      if (string.IsNullOrWhiteSpace(roomName))
        throw new ArgumentException($"GetRoomByName: '{nameof(roomName)}' cannot be null or whitespace.", nameof(roomName));

      var room = Rooms.FirstOrDefault(x => x.Name == roomName);
      if (room == null)
        throw new Exception($"Room '{roomName}' not found");

      return room;
    }

    /// <summary>
    /// Add attendees to atrium (awaiting assignment)
    /// </summary>
    /// <param name="attendee">Attendee to add to atrium</param>
    /// <returns>Newly added attendees</returns>
    internal Participant AddAttendee(Participant attendee)
    {
      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      attendee.SessionId = Guid.NewGuid().ToString();
      attendees.Add(attendee);

      Logger.LogInformation($"Adding attendee {attendee} to atrium '{Name}'");

      // respond with connection information
      SendConnectionStatus(attendee);

      // notify moderators in all rooms of new/modified atrium list
      BroadcastUnassignedAttendeeList();

      DumpAtrium();

      return attendee;
    }

    private void DumpAtrium()
    {
      int index = 0;
      Logger.LogDebug($"Atrium {Name}:");
      // transform list for moderator - make attendees a destination, not a source
      foreach (var attendee in attendees)
        Logger.LogDebug($"  [{index++}]: {attendee}");
    }

    /// <summary>
    /// Update an attendees connection id
    /// </summary>
    /// <param name="attendee">Attendee to update</param>
    /// <param name="conenctionId">SignalR connection Id</param>
    internal void UpdateAttendeeConnection(Participant attendee, string connectionId)
    {
      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      if (string.IsNullOrEmpty(connectionId))
        throw new ArgumentNullException(nameof(connectionId));

      attendee.ConnectionId = connectionId;
      SignalChangedAttendee(attendee);
    }

    /// <summary>
    /// Signal a changed attendee to room they are in
    /// </summary>
    /// <param name="attendee">Attendee to update</param>
    private void SignalChangedAttendee(Participant attendee)
    {
      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      var room = GetRoomByName(attendee.RoomName);

      var payload = new CommandReassignedPayload
      {
        Envelope = new Envelope
        {
          ToName = room.Moderator.ConnectionId,
          ToId = room.Moderator.Name,
          FromId = attendee.ConnectionId,
          FromName = attendee.Name,
          RoomName = room.Name
        },
        Data = attendee
      };

      Logger.LogDebug($"attendees {attendee.Name} reassigned to moderator {room.Moderator.Name}");
      SendMessageTo(payload, "command", JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Assign attendees to Room of atrium
    /// </summary>
    /// <param name="attendee">Participant to assign</param>
    public void AssignAttendee(Participant moderator, Participant attendee)
    {
      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      try
      {
        var serverModerator = GetModerator(moderator.ConnectionId);
        if (serverModerator == null)
          throw new Exception($"Cannot find server-side moderator {moderator.ConnectionId}");

        var serverAttendee = attendees.FirstOrDefault(x => x.ConnectionId == attendee.PartnerId);
        if (serverAttendee == null)
          throw new Exception($"Cannot find unassigned attendee {attendee}");

        Logger.LogInformation($"AssignAttendee: '{attendee}' to '{moderator}'");

        // test if attendees was already assigned (by another moderator?)
        if (serverAttendee.InChat)
          throw new Exception($"Attendee '{attendee.Name}' already assigned.");

        serverAttendee.ConnectionId = moderator.ConnectionId;
        serverAttendee.Name = moderator.Name;
        serverAttendee.InChat = true;
        serverAttendee.RoomName = moderator.RoomName;

        // remove attendee from unassigned since they should
        // be in a room now.
        attendees.Remove(serverAttendee);

        var payload = new CommandAssignedPayload
        {
          Envelope = new Envelope
          {
            FromId = moderator.ConnectionId,
            FromName = moderator.Name,
            ToName = attendee.PartnerName,
            ToId = attendee.PartnerId,
            RoomName = moderator.RoomName
          },
          Data = serverAttendee
        };

        Logger.LogDebug($"attendees {serverAttendee.Name} assigned to moderator {moderator.Name}");
        SendMessageTo(payload, "command", JsonSerializer.Serialize(payload));

        // update moderators of new unassigned list
        BroadcastUnassignedAttendeeList();

      }
      catch (Exception ex)
      {
        Logger.LogError($"AssignAttendee exception: {ex.Message}");

      }
    }

    /// <summary>
    /// Send connection status to participant
    /// </summary>
    /// <param name="hub">SignalR Hub</param>
    /// <param name="participant">Recipient</param>
    private void SendConnectionStatus(Participant participant)
    {
      if (participant is null)
        throw new ArgumentNullException(nameof(participant));

      // respond to attendees with status information
      var payload = new CommandStatusPayload
      {
        Envelope = new Envelope(participant),
        Data = participant
      };

      SendMessageTo(payload, "command", JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="methodName"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public void SendMessageTo(Payload payload, string methodName, string arg1, string arg2 = "")
    {
      if (payload is null)
        throw new ArgumentNullException(nameof(payload));

      if (string.IsNullOrEmpty(methodName))
        throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));

      if (string.IsNullOrEmpty(arg1))
        throw new ArgumentException($"'{nameof(arg1)}' cannot be null or empty.", nameof(arg1));

      try
      {
        Logger.LogDebug($"Send message to {payload.GetToId()}: {methodName}({arg1}, {arg2})");
        HubContext.Clients.Client(payload.GetToId()).SendAsync(methodName, arg1, arg2);
      }
      catch (Exception ex)
      {
        Logger.LogError($"SendMessageTo exception: {ex.Message}");
      }

    }

    /// <summary>
    /// Send unassigned attendees list all moderators
    /// </summary>
    public void BroadcastUnassignedAttendeeList()
    {
      foreach (var Room in Rooms)
      {
        // skip room if no moderator assigned to send list to
        Participant moderator = Room.GetModerator();
        if (moderator == null)
        {
          Logger.LogDebug($"No moderator for room '{Room.Name}' for unassigned attemdee update");
          continue;
        }

        var unassignedAttendees = new List<Participant>();

        // transform list for moderator - make attendees a destination, not a source
        foreach (var attendee in attendees)
        {
          var tempAttendee = new Participant
          {
            ConnectionId = moderator.ConnectionId,
            Name = moderator.Name,
            PartnerId = attendee.ConnectionId,
            PartnerName = attendee.Name
          };

          unassignedAttendees.Add(tempAttendee);
        }

        var payload = new CommandAttendeesPayload
        {
          Envelope = new Envelope(moderator),
          Data = unassignedAttendees
        };

        Logger.LogDebug($"Notifying {moderator} in room {Room.Name} of unassigned list update");

        SendMessageTo(payload, "command", JsonSerializer.Serialize(payload));
      }

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
      Participant attendee = null;

      // test if unassigned attendee exists
      if (attendees.Any(x => x.ConnectionId == connectionId))
      {
        attendees.Remove(attendees.First(x => x.ConnectionId == connectionId));
        Logger.LogDebug($"Removed attendee '{connectionId}' from '{Name}' unassigned list.");

        BroadcastUnassignedAttendeeList();
      }
      else
      {

        // get (hopefully) assigned attendee
        attendee = GetAttendee(connectionId);
        if (attendee == null)
        {
          Logger.LogError($"Attempted to disconnect attendees @ '{connectionId}' from '{Name}', but was not assigned");
          return false;
        }

        // get attendee room
        var Room = GetRoomContainingParticipant(attendee);
        if (Room == null)
        {
          Logger.LogError($"Cannot find attendee '{attendee}' room");
          return false;
        }

      }

      // TODO: remove attendee from room
      // TODO: tell moderator that attendee has gone
      DumpAtrium();

      return true;
    }

    /// <summary>
    /// Disconnect moderator from atrium
    /// </summary>
    /// <param name="connectionId">connection id to disconnect</param>
    public bool DisconnectModerator(string connectionId)
    {
      var moderator = GetModerator(connectionId);
      if (moderator == null)
      {
        Logger.LogError($"Attempted to disconnect moderator @ '{connectionId}', but was unknown");
        return false;
      }

      // get Room for moderator
      var Room = GetRoomContainingParticipant(moderator);
      if (Room == null)
      {
        Logger.LogError($"Cannot find Room for {moderator}");
        return false;
      }

      // TODO: add attendees to unassigned
      // TODO: remove Room

      return true;

    }
  }

}