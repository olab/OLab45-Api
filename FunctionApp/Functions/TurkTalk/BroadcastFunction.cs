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
    [Function("Broadcast")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction Broadcast([SignalRTrigger("Hub", "messages", "Broadcast", "message")] SignalRInvocationContext invocationContext, string message)
    {
      return new SignalRMessageAction("newMessage")
      {
        Arguments = new object[] { new NewMessage(invocationContext, message) }
      };
    }
  }
}
