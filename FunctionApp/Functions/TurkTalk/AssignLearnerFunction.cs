using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using OLab.Common.Exceptions;

namespace OLab.FunctionApp.Functions.SignalR
{
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
          DbContext,
          TtalkDbContext,
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
}
