using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// A connection was established with hubusing Microsoft.AspNetCore.SignalR;
    /// </summary>
    /// <returns></returns>
    public override Task OnConnectedAsync()
    {
      try
      {
        _logger.LogDebug($"OnConnectedAsync: '{ConnectionId.Shorten(Context.ConnectionId)}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnConnectedAsync exception: {ex.Message}");
      }

      return base.OnConnectedAsync();
    }
  }
}
