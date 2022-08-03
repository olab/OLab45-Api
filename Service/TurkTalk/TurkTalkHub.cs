using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  // [Route("olab/api/v3/turktalk")]
  public partial class TurkTalkHub : Hub
  {
    private readonly ILogger _logger;
    private readonly Conference _conference;

    /// <summary>
    /// TurkTalkHub constructor
    /// </summary>
    /// <param name="logger">Dependancy-injected logger</param>
    public TurkTalkHub(ILogger<TurkTalkHub> logger, Conference conference)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _conference = conference ?? throw new ArgumentNullException(nameof(conference));

      _logger.LogDebug($"TurkTalkHub ctor");
    }

    /// <summary>
    /// A connection was established with hubusing Microsoft.AspNetCore.SignalR;
    /// </summary>
    /// <returns></returns>
    public override Task OnConnectedAsync()
    {
      try
      {
        _logger.LogDebug($"OnConnectedAsync: incoming connection '{Context.ConnectionId}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnConnectedAsync exception: {ex.Message}");
      }

      return base.OnConnectedAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override Task OnDisconnectedAsync(Exception exception)
    {
      try
      {
        _conference.DisconnectSession(Context.ConnectionId);
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnDisconnectedAsync exception: {ex.Message}");
      }

      return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Handler for received messages
    /// </summary>
    /// <param name="payload">Message method payload</param>
    public void Message(MessagePayload payload)
    {
      try
      {
        _logger.LogDebug($"Message received '{payload.Data}', room {payload.Envelope.RoomName} from {payload.Envelope.FromId} -> {payload.Envelope.ToId}");

        var connectionId = Context.ConnectionId;

        //  echo message back to the sender
        var echoPayload = MessagePayload.GenerateEcho(payload);
        var room = _conference.GetRoom(payload.Envelope.RoomName);

        var senderParticipant = _conference.GetParticipantById(payload.Envelope.FromId, payload.Envelope.RoomName);

        _conference.SendMessageTo(echoPayload, "echo", JsonSerializer.Serialize(echoPayload));

        // send message to it's final destination
        _conference.SendMessageTo(payload, "message", JsonSerializer.Serialize(payload));

      }
      catch (Exception ex)
      {
        _logger.LogError($"Message exception: {ex.Message}");
      }
    }

    /// <summary>
    /// Broadcast message to all participants
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    public void BroadcastMessage(string sender, string message)
    {
      try
      {
        _logger.LogDebug($"Broadcast message received from '{sender}': '{message}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"BroadcastMessage exception: {ex.Message}");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="message"></param>
    public void Echo(string name, string message)
    {
      try
      {
        _logger.LogDebug($"Echo: '{name}' '{message}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Echo exception: {ex.Message}");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    protected void SendMessageAll(string method, string arg1, string arg2 = "")
    {
      try
      {

      }
      catch (Exception ex)
      {
        _logger.LogError($"SendMessageAll exception: {ex.Message}");
      }
      _logger.LogDebug($"broadcast: {method}({arg1}, {arg2})");
    }

  }
}
