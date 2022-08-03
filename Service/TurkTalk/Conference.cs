using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using OLabWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System;
using System.Text.Json;

namespace TurkTalk.Contracts
{
  public class Conference
  {
    public string Name { get; set; }

    public readonly ILogger logger;
    public readonly IHubContext<TurkTalkHub> hubContext;
    // private readonly Atrium _concierge;
    private readonly ConcurrentDictionary<string, Room> _rooms = new ConcurrentDictionary<string, Room>();

    public Conference(ILogger logger, IHubContext<TurkTalkHub> hubContext)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

      // _concierge = new Atrium(this, "Concierge|default");
      this.logger.LogDebug($"Conference ctor");
    }

    /// <summary>
    /// Get list of rooms
    /// </summary>
    /// <returns>List of Rooms</returns>
    public IList<Room> GetRooms()
    {
      return _rooms.Values.ToList();
    }

    /// <summary>
    /// Add attendee to conference
    /// </summary>
    /// <param name="connectionId">Attendee connectionId</param>
    /// <param name="participant">Attendee to add</param>
    /// <param name="roomName">Room name to join (in form '<node name>|<room name>')</param>
    internal void AddAttendee(string connectionId, Participant participant, string roomName)
    {
      // get room, create it if it doesn't exist
      var room = GetRoom(roomName, true);
      room.AddAttendee(connectionId, participant);
    }

    /// <summary>
    /// disconnects a session from the conference
    /// </summary>
    /// <param name="connectionId">Connection id to remove</param>
    internal void DisconnectSession(string connectionId)
    {
      logger.LogDebug($"OnDisconnectedAsync: removing '{connectionId}'");
      foreach (var room in GetRooms())
        room.DisconnectSession(connectionId);
    }

    /// <summary>
    /// Add moderator to conference
    /// </summary>
    /// <param name="connectionId">Attendee connectionId</param>
    /// <param name="moderator">Moderator to add</param>
    /// <param name="roomName">Room name to join (in form '<node name>|<room name>')</param>
    /// <param name="isBot">Moderator is a bot</param>
    internal void AddModerator(string connectionId, Participant moderator, string roomName, bool isBot)
    {
      // get atrium, create it if it doesn't exist
      var room = GetRoom(roomName, true);
      room.AddModerator(connectionId, moderator, isBot);
    }

    /// <summary>
    /// Send connection status to participant
    /// </summary>
    /// <param name="connectionId">Receivers connection id</param>
    /// <param name="participant">Recipient</param>
    public void SendConnectionStatus(string connectionId, Participant participant)
    {
      if (participant is null)
        throw new ArgumentNullException(nameof(participant));

      // respond to attendees with status information
      var payload = new CommandStatusPayload
      {
        Envelope = new Envelope(connectionId, participant),
        Data = participant
      };

      SendMessageTo(payload, "command", JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Send payload to participant
    /// </summary>
    /// <param name="payload">Payload to transmit</param>
    /// <param name="methodName">SignalR method name to invoke</param>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
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
        logger.LogDebug($"Send message to {payload.GetToId()}: {methodName}({arg1}, {arg2})");
        hubContext.Clients.Client(payload.GetToId()).SendAsync(methodName, arg1, arg2);
      }
      catch (Exception ex)
      {
        logger.LogError($"SendMessageTo exception: {ex.Message}");
      }

    }

    /// <summary>
    /// Get moderator from any Room
    /// </summary>
    /// <param name="key">Name or ConnectionId to look for</param>
    /// <returns>Participant</returns>
    public Participant GetModerator(string key)
    {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentException($"GetAttendee: '{nameof(key)}' cannot be null or empty.", nameof(key));

      Participant moderator = null;
      foreach (var room in GetRooms())
      {
        moderator = room.GetModerator(key);
        if (moderator != null)
          break;
      }

      if (moderator == null)
        logger.LogError($"GetModerator: moderator on '{key}' does not exist");

      return moderator;
    }

    /// <summary>
    /// Add unassigned attendee to room
    /// </summary>
    /// <param name="connectionId">Connection id of requestor</param>
    /// <param name="requestingSessionId">Session id of requestor</param>
    /// <param name="attendee">Attendee to add</param>
    /// <param name="roomName">Room name to join (in form '<node name>|<room name>')</param>
    public void AssignAttendee(string connectionId, string requestingSessionId, Participant attendee, string roomName)
    {
      // get roomatrium, create it if it doesn't exist
      var room = GetRoom(roomName, true);

      // get requesting moderator based on sessionId
      var moderator = room.GetModerator(requestingSessionId);

      // get server-side attendee since it has the connection Id
      attendee = room.GetAttendee(attendee);
      
      // assign attendee to moderator's room
      room.AssignAttendee(connectionId, moderator, attendee);
    }

    /// <summary>
    /// Get a room by name with optional create
    /// </summary>
    /// <param name="name">Name of Room</param>
    /// <param name="create">(Optional) create if doens't exist</param>
    /// <returns>Atrium or null</returns>
    public Room GetRoom(string name, bool create = false)
    {
      if (string.IsNullOrEmpty(name))
        throw new System.ArgumentException($"GetAtriumByName: '{nameof(name)}' cannot be null or empty.", nameof(name));

      if (_rooms.ContainsKey(name))
        return _rooms[name];

      if (create)
      {
        var room = new Room(this, name);
        _rooms.TryAdd(name, room);
        return _rooms[name];
      }

      throw new Exception($"Cannot find room '{name}'");
    }

    // TODO: complete this
    public Room OpenRoom(string name)
    {
      var room = GetRoom(name);
      // room.Open();
      return room;
    }

    // TODO: complete this
    public Room CloseRoom(string name)
    {
      var room = GetRoom(name);
      // room.Close();
      return room;
    }

    /// <summary>
    /// Get participant from room
    /// </summary>
    /// <param name="connectionId">Connection Id to look for</param>
    /// <param name="roomName">(optional) room name to look in</param>
    /// <returns>Participant</returns>
    internal Participant GetParticipantById(string connectionId, string roomName = "")
    {
      if (string.IsNullOrEmpty(connectionId))
        throw new System.ArgumentException($"GetParticipantById: '{nameof(connectionId)}' cannot be null or empty.", nameof(connectionId));

      Participant participant;

      try
      {
        // if no room provided - search thru all rooms
        if (roomName.Length == 0)
        {
          foreach (var room in GetRooms())
          {
            participant = room.GetAttendee(connectionId);
            if (participant != null)
              return participant;
          }

          return null;
        }

        else
        {
          // look for connectionId in specific room
          var room = GetRoom(roomName);
          if (room == null)
          {
            logger.LogError($"GetParticipantById: room '{roomName}' not found");
            return null;
          }

          participant = room.GetAttendee(connectionId);
        }

      }
      catch (System.Exception ex)
      {
        logger.LogError(ex, "GetParticipantById exception");
        participant = null;
      }

      return participant;

    }

  }

}