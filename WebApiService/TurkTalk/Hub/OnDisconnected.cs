﻿using Common.Utils;
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
    /// A connection is disconnected from the Hub
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
      try
      {
        _logger.LogDebug($"OnDisconnectedAsync: '{ConnectionId.Shorten(Context.ConnectionId)}'");

        var participant = new Participant(Context);

        // we don't know which user disconnected, so we have to search
        // the known topics by SignalR context
        foreach (Topic topic in _conference.Topics)
          await topic.RemoveParticipantAsync(participant);
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnDisconnectedAsync exception: {ex.Message}");
      }

      await base.OnDisconnectedAsync(exception);
    }
  }
}