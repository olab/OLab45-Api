using Microsoft.Azure.Functions.Worker;
using OLab.TurkTalk.Endpoints;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{
  [Function("Message")]
  [SignalROutput(HubName = "Hub")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
  public async Task<IList<object>> Message(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    [SignalRTrigger("Hub", "messages", "Message", "payload")] SignalRInvocationContext invocationContext,
    SendMessageRequest payload)
  {
    try
    {
      payload.ConnectionId = invocationContext.ConnectionId;
      // decrypt the user token from the payload
      payload.DecryptAndRefreshUserToken(_configuration.GetAppSettings().Secret);

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        _conference);

      endpoint.SendMessage(payload);

      Logger.LogInformation(JsonSerializer.Serialize(endpoint.MessageQueue.Messages));
      return endpoint.MessageQueue.Messages;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
      throw;
    }

  }
}
