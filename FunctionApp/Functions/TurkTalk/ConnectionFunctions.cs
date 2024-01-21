using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{

  [Function("OnConnected")]
  [SignalROutput(HubName = "Hub")]
  public SignalRMessageAction OnConnected(
    [SignalRTrigger("Hub", "connections", "connected")] SignalRInvocationContext invocationContext)
  {
    //invocationContext.Headers.TryGetValue("Authorization", out var auth);
    Logger.LogInformation($"{invocationContext.ConnectionId} has connected");

    var endpoint = new TurkTalkEndpoint(
      Logger,
      _configuration,
      _conference);

    try
    {

      if (invocationContext.Query.TryGetValue("olab_access_token", out var accessToken))
      {
        var auth = new OLabAuthentication(Logger, _configuration, DbContext);

        if (!invocationContext.Query.TryGetValue("sessionId", out var sessionId))
          return endpoint.OnNotAuthenticated(
            _configuration,
            invocationContext.ConnectionId,
            "Session Id was not provided");

        auth.ValidateToken(accessToken);

        var physParticipant = endpoint.GetParticipant(auth, sessionId);

        return endpoint.OnAuthenticated(
          _configuration,
          invocationContext.ConnectionId,
          physParticipant,
          auth);

      }

      return endpoint.OnNotAuthenticated(
        _configuration,
        invocationContext.ConnectionId,
        "Access token was not provided");

    }
    catch (System.Exception ex)
    {
      return endpoint.OnNotAuthenticated(
        _configuration,
        invocationContext.ConnectionId,
        ex.Message);

    }

  }


  [Function("OnDisconnected")]
  [SignalROutput(HubName = "Hub")]
  public async Task<DispatchedMessages> OnDisconnected(
    [SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
  {
    Logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");

    var payload = new OnDisconnectedRequest();
    payload.ConnectionId = invocationContext.ConnectionId;

    var endpoint = new TurkTalkEndpoint(
      Logger,
      _configuration,
      _conference);

    return await endpoint.OnDisconnectedAsync(
      _configuration,
      invocationContext.ConnectionId,
      payload);

  }
}
