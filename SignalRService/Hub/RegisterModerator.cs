using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.Mvc;

namespace OLab.SignalRService.Api
{
  public partial class Hub : ServerlessHub
  {
    [FunctionName(nameof(RegisterModerator))]
    public IActionResult RegisterModerator(
        [SignalRTrigger] InvocationContext invocationContext,
        ILogger logger,
        CancellationToken token)
    {

      try
      {
        logger.LogInformation($"RegisterModerator connectionId {invocationContext.ConnectionId}");
        return new JsonResult( invocationContext );

      }
      catch (System.Exception ex)
      {
        logger.LogError(ex, nameof(RegisterModerator));
        throw;
      }

    }

  }
}
