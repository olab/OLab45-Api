using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects.API;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class OnConnectedFunction : OLabFunction
  {

    [Function("OnConnected")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction OnConnected([SignalRTrigger("Hub", "connections", "connected")] SignalRInvocationContext invocationContext)
    {
      invocationContext.Headers.TryGetValue("Authorization", out var auth);
      Logger.LogInformation($"{invocationContext.ConnectionId} has connected");

      return new SignalRMessageAction("newConnection")
      {
        Arguments = new object[] { new NewConnection(invocationContext.ConnectionId, auth) },

      };
    }

  }
}
