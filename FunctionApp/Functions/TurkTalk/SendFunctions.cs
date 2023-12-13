using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Models;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("Broadcast")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction Broadcast([SignalRTrigger("Hub", "messages", "Broadcast", "message")] SignalRInvocationContext invocationContext, string message)
    {
      return new SignalRMessageAction("newMessage")
      {
        Arguments = new object[] { new NewMessage(invocationContext, message) }
      };
    }

    [Function("SendToUser")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction SendToUser([SignalRTrigger("Hub", "messages", "SendToUser", "userName", "message")] SignalRInvocationContext invocationContext, string userName, string message)
    {
      return new SignalRMessageAction("newMessage")
      {
        UserId = userName,
        Arguments = new object[] { new NewMessage(invocationContext, message) }
      };
    }

    [Function("SendToConnection")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction SendToConnection([SignalRTrigger("Hub", "messages", "SendToConnection", "connectionId", "message")] SignalRInvocationContext invocationContext, string connectionId, string message)
    {
      return new SignalRMessageAction("newMessage")
      {
        ConnectionId = connectionId,
        Arguments = new object[] { new NewMessage(invocationContext, message) }
      };
    }
  }
}
