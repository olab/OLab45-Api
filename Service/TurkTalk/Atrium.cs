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
    public IList<Participant> Attendees = new List<Participant>();

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

      if (string.IsNullOrEmpty(key))
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

      if (string.IsNullOrEmpty(key))
        return null;

      // look for participant in atrium first
      attendee = Attendees.FirstOrDefault(x => x.SessionId == key);

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
        if ((Room.GetAttendee(participant.SessionId) != null) ||
             (Room.GetModerator(participant.SessionId) != null))
          return Room;
      }

      return null;
    }

    /// <summary>
    /// Add moderator to atrium
    /// </summary>
    /// <param name="senderConnectionId">Connection Id to respond to</param>
    /// <param name="moderator">Moderator to add to atrium</param>
    /// <returns>Newly added moderator</returns>
    internal Participant AddModerator(string senderConnectionId, Participant moderator)
    {
      if (moderator is null)
        throw new ArgumentNullException(nameof(moderator));

      var penModerator = GetModerator(moderator.SessionId);
      if (penModerator != null)
      {
        Logger.LogWarning($"Moderator {penModerator.Name} already connected");
        return penModerator;
      }

      var Room = GetUnmoderatedRoom(true);
      Room.SetModerator(moderator);

      Logger.LogInformation($"Adding moderator {moderator} to atrium '{Room.Name}'");

      moderator.RoomName = Room.Name;
      moderator.InChat = true;

      // respond to moderator with status information
      SendConnectionStatus(senderConnectionId, moderator);

      // send unassigned attendees list to moderators
      BroadcastUnassignedAttendeeList();

      return moderator;
    }

    internal void UpdateModerator(string senderConnectionId, Participant moderator)
    {
      var serverModerator = GetModerator(moderator.SessionId);
      if (serverModerator == null)
      {
        Logger.LogError($"Moderator {moderator} not found under atrium '{Name}'");
        return;
      }

      serverModerator.SetConnectionId(senderConnectionId);

      Logger.LogInformation($"Updated moderator {moderator} in room '{moderator.RoomName}'");

      DumpAtrium();

      // respond to attendee with connection information
      SendConnectionStatus(senderConnectionId, moderator);
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
    /// <param name="senderConnectionId">Connection Id to respond to</param>
    /// <param name="attendee">Attendee to add to atrium</param>
    /// <returns>Newly added attendees</returns>
    internal Participant AddAttendee(string senderConnectionId, Participant attendee)
    {
      if (attendee is null)
        throw new ArgumentNullException(nameof(attendee));

      Attendees.Add(attendee);
      DumpAtrium();

      Logger.LogInformation($"Adding attendee {attendee}({attendee.SessionId.Substring(0, 3)}) to atrium '{Name}'");

      // respond to attendee with connection information
      SendConnectionStatus(senderConnectionId, attendee);

      // notify moderators in all rooms of new/modified atrium list
      BroadcastUnassignedAttendeeList();

      return attendee;
    }

    /// <summary>
    /// Updates attendee
    /// </summary>
    /// <param name="senderConnectionId">Connection Id to respond to</param>
    /// <param name="attendee">Attendee to add to atrium</param>
    internal void UpdateAttendee(string senderConnectionId, Participant attendee)
    {
      var serverAttendee = GetAttendee(attendee.SessionId);
      if (serverAttendee == null)
      {
        Logger.LogError($"Attendee {attendee} not found under atrium '{Name}'");
        return;
      }

      serverAttendee.SetConnectionId(senderConnectionId);

      Logger.LogInformation($"Updated attendee {attendee} to atrium '{Name}'");

      DumpAtrium();

      // respond to attendee with connection information
      SendConnectionStatus(senderConnectionId, attendee);
      // notify moderators in all rooms of new/modified atrium list
      BroadcastUnassignedAttendeeList();

    }

    public void DumpAtrium()
    {
      int index = 0;
      Logger.LogDebug($"Atrium {Name}:");

      foreach (var room in Rooms)
        Logger.LogDebug($"  [{room.Name}]: {room.Moderator}");

      foreach (var attendee in Attendees)
        Logger.LogDebug($"  [{index++}]: {attendee}");
    }

    /// <summary>
    /// Update an participant connection id
    /// </summary>
    /// <param name="participant">Attendee to update</param>
    /// <param name="conenctionId">SignalR connection Id</param>
    internal void UpdateParticipantSession(string connectionId, Participant participant)
    {
      if (participant is null)
        throw new ArgumentNullException(nameof(participant));

      if (string.IsNullOrEmpty(connectionId))
        throw new ArgumentNullException(nameof(connectionId));

      participant.SessionId = connectionId;
      SignalChangedAttendee(participant);
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
          ToName = room.Moderator.SessionId,
          ToId = room.Moderator.Name,
          FromId = attendee.SessionId,
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
        var serverModerator = GetModerator(moderator.SessionId);
        if (serverModerator == null)
          throw new Exception($"Cannot find server-side moderator {moderator.SessionId}");

        var serverAttendee = Attendees.FirstOrDefault(x => x.SessionId == attendee.PartnerSessionId);
        if (serverAttendee == null)
          throw new Exception($"Cannot find unassigned attendee {attendee}");

        Logger.LogInformation($"AssignAttendee: '{attendee}' to '{moderator}'");

        // test if attendees was already assigned (by another moderator?)
        if (serverAttendee.InChat)
          throw new Exception($"Attendee '{attendee.Name}' already assigned.");

        serverAttendee.SessionId = moderator.SessionId;
        serverAttendee.Name = moderator.Name;
        serverAttendee.InChat = true;
        serverAttendee.RoomName = moderator.RoomName;

        // remove attendee from unassigned since they should
        // be in a room now.
        Attendees.Remove(serverAttendee);

        var payload = new CommandAssignedPayload( moderator, attendee, serverAttendee );

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
    /// <param name="senderConnectionId">Connection Id to respond to</param>
    /// <param name="participant">Recipient</param>
    private void SendConnectionStatus(string senderConnectionId, Participant participant)
    {
      if (participant is null)
        throw new ArgumentNullException(nameof(participant));

      // respond to attendees with status information
      var payload = new CommandConnectionStatusPayload(senderConnectionId, participant);

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
        Logger.LogDebug($"SendMessageTo {payload.GetToId().Substring(0, 3)}: {methodName}({arg1}, {arg2})");
        HubContext.Clients.Client(payload.GetToId()).SendAsync(methodName, arg1, arg2);
      }
      catch (Exception ex)
      {
        Logger.LogError($"SendMessageTo exception: {ex.Message}");
      }

    }

    /// <summary>
    /// Send unassigned attendees list all moderators
    /// <param name="senderConnectionId">Connection Id to respond to</param>
    /// </summary>
    public void BroadcastUnassignedAttendeeList()
    {
      if (Attendees.Count() == 0)
      {
        Logger.LogDebug($"No unassigned attendees to broadcast updates on");
        return;
      }

      foreach (var room in Rooms)
      {
        // skip room if no moderator assigned to send list to
        Participant moderator = room.GetModerator();
        if (moderator == null)
        {
          Logger.LogDebug($"No moderator for room '{room.Name}' skipping unassigned broadcast");
          continue;
        }

        var unassignedAttendees = new List<Participant>();

        // transform list for moderator - make attendees a destination, not a source
        foreach (var attendee in Attendees)
        {
          var unassignedAttendee = new UnassignedParticipant(room, moderator, attendee);
          unassignedAttendees.Add(unassignedAttendee);
        }

        var payload = new CommandAttendeesPayload(moderator, unassignedAttendees);

        Logger.LogDebug($"Sending {room.Name}/{moderator} unassigned broadcast");
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
    /// <param name="sessionId">connection id to disconnect</param>
    /// <returns>true/false</returns>
    public bool DisconnectAttendee(string sessionId)
    {
      Participant attendee = null;

      // test if unassigned attendee exists
      if (Attendees.Any(x => x.SessionId == sessionId))
      {
        Attendees.Remove(Attendees.First(x => x.SessionId == sessionId));
        Logger.LogDebug($"Removed attendee '{sessionId}' from '{Name}' unassigned list.");

        BroadcastUnassignedAttendeeList();
      }
      else
      {

        // get (hopefully) assigned attendee
        attendee = GetAttendee(sessionId);
        if (attendee == null)
        {
          Logger.LogError($"Attempted to disconnect attendees @ '{sessionId}' from '{Name}', but was not assigned");
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