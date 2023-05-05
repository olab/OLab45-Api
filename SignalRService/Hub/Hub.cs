using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Dawn;
using OLab.SignalRService.BusinessObjects;

namespace OLab.SignalRService.Api
{
  public partial class Hub : ServerlessHub
  {
    private static Conference _conference;

    private static Conference BuildConference(ILogger logger)
    {
      _conference ??= new Conference(logger);
      return _conference;
    }

    private static string GetEnvironmentVariable(string name, string defaultValue, ILogger logger)
    {
      string value;
      try
      {
        value = GetEnvironmentVariable(name, logger);
      }
      catch (System.ArgumentNullException)
      {
        return defaultValue;
      }

      return value;
    }

    private static string GetEnvironmentVariable(string name, ILogger logger)
    {
      Guard.Argument(name).NotEmpty(nameof(name));
      var variable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
      if (string.IsNullOrEmpty(variable))
        throw new ArgumentNullException(name);

      return variable;
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected(
        [SignalRTrigger] InvocationContext invocationContext,
        ILogger logger,
        CancellationToken token
    )
    {
      invocationContext.Headers.TryGetValue("Authorization", out var auth);
      logger.LogInformation($"OnConnected {invocationContext.ConnectionId}");
    }

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected(
        [SignalRTrigger] InvocationContext invocationContext,
        ILogger logger,
        CancellationToken token)
    {
      logger.LogInformation($"OnDisconnected {invocationContext.ConnectionId}");
    }

  }
}
