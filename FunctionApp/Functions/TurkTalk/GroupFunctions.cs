using Microsoft.Azure.Functions.Worker;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{
  [Function("JoinGroup")]
  [SignalROutput(HubName = "Hub")]
  public SignalRGroupAction JoinGroup(
    [SignalRTrigger("Hub", "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext,
    string connectionId,
    string groupName)
  {
    return new SignalRGroupAction(SignalRGroupActionType.Add)
    {
      GroupName = groupName,
      ConnectionId = connectionId
    };
  }

  [Function("LeaveGroup")]
  [SignalROutput(HubName = "Hub")]
  public SignalRGroupAction LeaveGroup(
    [SignalRTrigger("Hub", "messages", "LeaveGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext,
    string connectionId,
    string groupName)
  {
    return new SignalRGroupAction(SignalRGroupActionType.Remove)
    {
      GroupName = groupName,
      ConnectionId = connectionId
    };
  }

  [Function("JoinUserToGroup")]
  [SignalROutput(HubName = "Hub")]
  public SignalRGroupAction JoinUserToGroup(
    [SignalRTrigger("Hub", "messages", "JoinUserToGroup", "userName", "groupName")] SignalRInvocationContext invocationContext,
    string userName,
    string groupName)
  {
    return new SignalRGroupAction(SignalRGroupActionType.Add)
    {
      GroupName = groupName,
      UserId = userName
    };
  }

  [Function("LeaveUserFromGroup")]
  [SignalROutput(HubName = "Hub")]
  public SignalRGroupAction LeaveUserFromGroup(
    [SignalRTrigger("Hub", "messages", "LeaveUserFromGroup", "userName", "groupName")] SignalRInvocationContext invocationContext,
    string userName,
    string groupName)
  {
    return new SignalRGroupAction(SignalRGroupActionType.Remove)
    {
      GroupName = groupName,
      UserId = userName
    };
  }
}
