using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;
using OLab.FunctionApp.Utils;
using System.Collections.Generic;
using System.Security.Claims;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {

    [Function("OnConnected")]
    [SignalROutput(HubName = "Hub")]
    public SignalRMessageAction OnConnected([SignalRTrigger("Hub", "connections", "connected")] SignalRInvocationContext invocationContext)
    {
      //invocationContext.Headers.TryGetValue("Authorization", out var auth);
      Logger.LogInformation($"{invocationContext.ConnectionId} has connected");

      if (invocationContext.Query.TryGetValue("olab_access_token", out var accessToken))
      {
        var auth = new OLabAuthentication(Logger, _configuration, DbContext);
        auth.ValidateToken(accessToken);

        return new SignalRMessageAction("newConnection")
        {
          Arguments = new object[] { new NewConnection(
            _configuration, 
            invocationContext.ConnectionId, 
            auth) },
        };

      }

      return new SignalRMessageAction("newConnection")
      {
        Arguments = new object[] { "fail" }
      };
    }

    public class NewConnection
    {
      public string ConnectionId { get; }

      public string UserKey { get; }

      public NewConnection(
        IOLabConfiguration configuration,
        string connectionId, 
        IOLabAuthentication auth)
      {
        ConnectionId = connectionId;
        UserKey = new UserInfoEncoder().EncryptUser(
          configuration.GetAppSettings().Secret,
          auth.Claims["id"],
          auth.Claims[ClaimTypes.Name],
          auth.Claims["name"],
          auth.Claims["iss"]);
      }
    }

    [Function("OnDisconnected")]
    [SignalROutput(HubName = "Hub")]
    public void OnDisconnected([SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
    {
      Logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
    }
  }
}
