using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using Dawn;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
  public class Session
  {
    private readonly Conference _conference;
    private IList<Room> _rooms;
    // Needed because there's no such thing as a thread-safe List<>.
    private static Mutex mut = new Mutex();
    
    public Session(Conference conference)
    {
      Guard.Argument(conference).NotNull(nameof(conference));
      
      _conference = conference;
      _rooms = new List<Room>();
    }

    protected ILogger logger {
      get { return _conference.Logger; }
    }

    /// <summary>
    /// Get number of rooms in session
    /// </summary>
    /// <returns>Room count</returns>
    public int RoomCount()
    {
      mut.WaitOne();
      var count = _rooms.Count;
      mut.ReleaseMutex();

      return count;
    }

    /// <summary>
    /// Get session room by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Room</returns>
    public Room GetRoom( int index )
    {
      mut.WaitOne();
      var room = _rooms[ index ];
      mut.ReleaseMutex();

      return room;
    }
  }
}