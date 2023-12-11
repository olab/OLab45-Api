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
    [Function("SendToGroup")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction SendToGroup([SignalRTrigger("Hub", "messages", "SendToGroup", "groupName", "message")] SignalRInvocationContext invocationContext, string groupName, string message)
    {
      return new SignalRMessageAction("newMessage")
      {
        GroupName = groupName,
        Arguments = new object[] { new NewMessage(invocationContext, message) }
      };
    }

    [Function("JoinGroup")]
    [SignalROutput(HubName = "Hub")]
    public SignalRGroupAction JoinGroup([SignalRTrigger("Hub", "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
    {
      return new SignalRGroupAction(SignalRGroupActionType.Add)
      {
        GroupName = groupName,
        ConnectionId = connectionId
      };
    }

    [Function("LeaveGroup")]
    [SignalROutput(HubName = "Hub")]
    public SignalRGroupAction LeaveGroup([SignalRTrigger("Hub", "messages", "LeaveGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
    {
      return new SignalRGroupAction(SignalRGroupActionType.Remove)
      {
        GroupName = groupName,
        ConnectionId = connectionId
      };
    }

    [Function("JoinUserToGroup")]
    [SignalROutput(HubName = "Hub")]
    public SignalRGroupAction JoinUserToGroup([SignalRTrigger("Hub", "messages", "JoinUserToGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
    {
      return new SignalRGroupAction(SignalRGroupActionType.Add)
      {
        GroupName = groupName,
        UserId = userName
      };
    }

    [Function("LeaveUserFromGroup")]
    [SignalROutput(HubName = "Hub")]
    public SignalRGroupAction LeaveUserFromGroup([SignalRTrigger("Hub", "messages", "LeaveUserFromGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
    {
      return new SignalRGroupAction(SignalRGroupActionType.Remove)
      {
        GroupName = groupName,
        UserId = userName
      };
    }
  }
}
