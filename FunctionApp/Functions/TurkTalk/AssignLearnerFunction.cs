using Microsoft.Azure.Functions.Worker;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{
  [Function("AssignLearner")]
  [SignalROutput(HubName = "Hub")]
  public async Task<IList<object>> AssignLearner(
    [SignalRTrigger("Hub", "messages", "AssignLearner", "payload")] SignalRInvocationContext invocationContext,
    AssignLearnerRequest payload)
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

      await endpoint.AssignLearnerAsync(payload);

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
