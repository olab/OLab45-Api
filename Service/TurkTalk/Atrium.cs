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
    private Room _room;
    public readonly ILogger logger;

    private IList<Participant> _unassignedAttendees = new List<Participant>();

    public Atrium(Room room)
    {
      _room = room ?? throw new ArgumentNullException(nameof(room));
      logger = room.logger ?? throw new ArgumentNullException(nameof(room.logger));
      logger.LogInformation($"Created atrium");
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
        logger.LogDebug($"removing '{attendee}' from unassigned list");
        _unassignedAttendees.Remove(attendee);
      }

      return attendee != null;
    }

    internal void AddAttendee(Participant attendee)
    {
      attendee.IsAssigned = false;
      _unassignedAttendees.Add(attendee);
    }
  }

}