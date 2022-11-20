using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Dawn;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
  public class Room
  {
    private readonly Session _session;

    public Room(Session session)
    {
      Guard.Argument(session).NotNull(nameof(session));
      _session = session;
    }
  }
}