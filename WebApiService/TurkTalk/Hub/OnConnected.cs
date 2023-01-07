using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Data;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Services.TurkTalk.Venue;
using Serilog.Core;
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
        ContextId = Convert.ToString(Context.GetHttpContext().Request.Query["contextId"]);
        var request = Context.GetHttpContext().Request;

        var accessToken = $"Bearer {Convert.ToString(Context.GetHttpContext().Request.Query["access_token"])}";
        request.Headers.Add("Authorization", accessToken);

        UserContext userContext = new UserContext(_logger, DbContext, request);

        _logger.LogDebug($"OnConnectedAsync: '{ConnectionId.Shorten(Context.ConnectionId)}'. ContextId: '{ContextId}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnConnectedAsync exception: {ex.Message}");
      }

      return base.OnConnectedAsync();
    }
  }
}
