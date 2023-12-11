using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
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

    [Function("OnDisconnected")]
    [SignalROutput(HubName = "Hub")]
    public void OnDisconnected([SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
    {
      Logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
    }

    public class NewConnection
    {
      public string ConnectionId { get; }

      public string Authentication { get; }

      public NewConnection(string connectionId, string auth)
      {
        ConnectionId = connectionId;
        Authentication = auth;
      }
    }
  }
}
