using Common.Utils;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{

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
      payload);

  }
}
