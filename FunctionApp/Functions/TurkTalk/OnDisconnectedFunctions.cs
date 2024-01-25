using Azure.Messaging.EventGrid;
using Common.Utils;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{

#if DEBUG

  [Function("OnDisconnected")]
  [SignalROutput(HubName = "Hub")]
  public async Task<SignalRMessageAction> OnDisconnectedSignalR(
    [SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
  {
    Logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");

    var status = new SignalRConnectionStatus
    {
      ConnectionId = invocationContext.ConnectionId
    };

    // pass thru the event to the EventGridEvent-aware message
    var eventGridEvent = new EventGridEvent("", "", "", status );

    var payload = new OnDisconnectedRequest();
    payload.ConnectionId = invocationContext.ConnectionId;

    return await OnDisconnectedEvent(eventGridEvent);
  }

#endif

}
