using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OLab.SignalRService.BusinessObjects
{
  public class Conference
  {
    private readonly ILogger _logger;
    public IDictionary<string, Room> Rooms;

    public int Count {
      get { return Rooms.Count; }
    }

    public Conference(ILogger logger)
    {
      Rooms = new ConcurrentDictionary<string, Room>();
      _logger = logger;
    }

    public static string MakeRoomKey(uint mapId, uint nodeId, int sequenceNumber)
    {
      return $"room/{mapId}/{nodeId}/{sequenceNumber}";
    }

    /// <summary>
    /// Create/Add new room
    /// </summary>
    /// <returns>SignalR group name</returns>
    public string CreateRoom(uint mapId, uint nodeId)
    {
      try
      {
        var room = new Room();
        var name = MakeRoomKey(mapId, nodeId, Rooms.Count);

        Rooms.Add(name, room);

        _logger.LogInformation($"Created room '{name}'");
        return name;

      }
      catch (System.Exception ex)
      {
        _logger.LogError(ex, "Error in CreateRoom", mapId, nodeId);
      }

      return null;
    }
  }
}