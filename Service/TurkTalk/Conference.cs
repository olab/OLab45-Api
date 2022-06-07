using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using OLabWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TurkTalk.Contracts
{
  public class Conference
  {
    public string Name { get; set; }

    public readonly ILogger Logger;
    public readonly IHubContext<TurkTalkHub> HubContext;
    private readonly Atrium _conciergeRoom;
    private readonly ConcurrentDictionary<string, Atrium> _atriums = new ConcurrentDictionary<string, Atrium>();

    public Conference(ILogger logger, IHubContext<TurkTalkHub> hubContext)
    {
      Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
      HubContext = hubContext ?? throw new System.ArgumentNullException(nameof(hubContext));

      _conciergeRoom = new Atrium(this, "Concierge");
      Logger.LogDebug($"Conference ctor");
    }

    /// <summary>
    /// Get list of rooms in atrium
    /// </summary>
    /// <returns>List of atrium names</returns>
    public IList<string> GetAtriums()
    {
      var names = _atriums.Values.OrderBy(x => x.Name).Select(x => x.Name).ToList();
      return names;
    }

    /// <summary>
    /// Get an atrium
    /// </summary>
    /// <param name="name">Name of atrium</param>
    /// <param name="create">(Optional) create if doens't exist</param>
    /// <returns>Atrium or null</returns>
    public Atrium GetAtriumByName(string name, bool create = false)
    {
      if (string.IsNullOrEmpty(name))
        throw new System.ArgumentException($"GetAtriumByName: '{nameof(name)}' cannot be null or empty.", nameof(name));

      if (_atriums.ContainsKey(name))
        return _atriums[name];
      if (create)
        return CreateAtrium(name);

      Logger.LogError($"GetAtriumByName: atrium '{name}' not found");
      return null;
    }

    /// <summary>
    /// Create new atrium
    /// </summary>
    /// <param name="name">Atrium name</param>
    /// <returns>Atrium</returns>
    public Atrium CreateAtrium(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new System.ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

      var atrium = new Atrium(this, name);
      _atriums.TryAdd(name, atrium);
      return atrium;
    }

    public Atrium OpenAtrium(string name)
    {
      var atrium = GetAtriumByName(name);
      atrium.Open();
      return atrium;
    }

    public Atrium CloseAtrium(string name)
    {
      var atrium = GetAtriumByName(name);
      atrium.Close();
      return atrium;
    }

    /// <summary>
    /// Get participant from atrium/room
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
        var parts = roomName.Split("//");

        // test if no atrium/room provided - search thru all atriums
        if (parts.Count() == 0)
        {
          foreach (var item in _atriums.Values)
          {
            participant = item.GetAttendee(connectionId);
            if (participant != null)
              return participant;
          }

          return null;
        }

        // look for connectionId in specific atrium
        var atriumName = parts[0];
        var atrium = GetAtriumByName(atriumName);
        if (atrium == null)
        {
          Logger.LogError($"GetParticipantById: atrium '{atriumName}' not found");
          return null;
        }

        participant = atrium.GetAttendee(connectionId);

      }
      catch (System.Exception ex)
      {
        Logger.LogError(ex, "GetParticipantById exception");
        participant = null;
      }

      return participant;

    }

    /// <summary>
    /// Gets a room by name
    /// </summary>
    /// <param name="roomName">Room name (<atrium_name>:<instance_number>)</param>
    /// <returns>Room</returns>
    public Room GetRoomByName(string roomName)
    {
      if (string.IsNullOrEmpty(roomName))
        throw new System.ArgumentException($"GetRoomByName: '{nameof(roomName)}' cannot be null or empty.", nameof(roomName));

      var parts = roomName.Split("//");
      if (parts.Count() == 0)
        throw new System.ArgumentException($"GetRoomByName: '{nameof(roomName)}' is invalid.", nameof(roomName));

      var atriumName = parts[0];
      var atrium = GetAtriumByName(atriumName);
      if (atrium == null)
        return null;

      return atrium.GetRoomByName(roomName);
    }
  }

}