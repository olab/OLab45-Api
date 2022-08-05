using TurkTalk.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OLabWebAPI.Services
{
  public class Atrium
  {
    private Room _room;
    public readonly ILogger logger;

    private IList<Participant> _unassignedAttendees = new List<Participant>();

    public Atrium(Room room)
    {
      _room = room ?? throw new ArgumentNullException(nameof(room));
      logger = room.logger ?? throw new ArgumentNullException(nameof(room.logger));
      logger.LogInformation($"Created atrium for room '{room.Name}'");
    }

    internal void Open()
    {
      throw new NotImplementedException();
    }

    internal void Close()
    {
      throw new NotImplementedException();
    }

    internal IList<Participant> GetUnassignedAttendees()
    {
      return _unassignedAttendees;
    }

    /// <summary>
    /// Remove attendee from unassigned list
    /// </summary>
    /// <param name="connectionId">Connection id to look for</param>
    internal bool RemoveAttendee(string connectionId)
    {
      var attendee = _unassignedAttendees.FirstOrDefault(x => x.IsIdentifiedBy(connectionId));
      if (attendee != null)
      {
        _unassignedAttendees.Remove(attendee);
        logger.LogDebug($"removed '{attendee}' from '{_room.Name}' unassigned list");
      }

      return attendee != null;
    }

    internal void AddAttendee(Participant attendee)
    {
      attendee.IsAssigned = false;
      _unassignedAttendees.Add(attendee);
      logger.LogDebug($"added attendee '{attendee}' to '{_room.Name}' atrium");
    }
  }

}