using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("RegisterLearner")]
    [SignalROutput(HubName = "Hub")]
    public async Task<IList<object>> RegisterLearner(
      [SignalRTrigger("Hub", "messages", "RegisterLearner", "payload")] SignalRInvocationContext invocationContext,
      RegisterParticipantRequest payload)
    {
      payload.ConnectionId = invocationContext.ConnectionId;
      // decrypt the user token from the payload
      payload.RefreshUserToken(_configuration.GetAppSettings().Secret);

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        DbContext,
        TtalkDbContext,
        _conference);

      await endpoint.RegisterLearnerAsync(payload);

      Logger.LogInformation(JsonSerializer.Serialize(endpoint.MessageQueue.Messages));
      return endpoint.MessageQueue.Messages;
    }
  }
}
