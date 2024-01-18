using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.TurkTalk.Endpoints;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{

  [Function("OnConnected")]
  [SignalROutput(HubName = "Hub")]
  public SignalRMessageAction OnConnected([SignalRTrigger("Hub", "connections", "connected")] SignalRInvocationContext invocationContext)
  {
    //invocationContext.Headers.TryGetValue("Authorization", out var auth);
    Logger.LogInformation($"{invocationContext.ConnectionId} has connected");

    if (invocationContext.Query.TryGetValue("olab_access_token", out var accessToken))
    {
      var auth = new OLabAuthentication(Logger, _configuration, DbContext);
      auth.ValidateToken(accessToken);

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        _conference);

      return endpoint.OnConnected(
        _configuration,
        invocationContext.ConnectionId,
        auth);
    }

    return new SignalRMessageAction("newConnection")
    {
      Arguments = new object[] { "fail" }
    };
  }

  [Function("OnDisconnected")]
  [SignalROutput(HubName = "Hub")]
  public void OnDisconnected([SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
  {
    Logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
  }
}
