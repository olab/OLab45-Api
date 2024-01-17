using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Utils;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Middleware;

public class TTalkAuthMiddleware : IFunctionsWorkerMiddleware
{

  public IOLabAuthentication _authentication { get; private set; }
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public TTalkAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _logger = OLabLogger.CreateNew<TTalkAuthMiddleware>(loggerFactory);
    _logger.LogInformation("TTalkAuthMiddleware created");

    _config = configuration;
  }

  public static bool CanInvoke(FunctionContext context)
  {
    var contextInfo = context.Items["contextHelper"] as ContextHelper;
    Guard.Argument(contextInfo).NotNull(nameof(contextInfo));

    if (contextInfo.InputBindings.ContainsKey("invocationContext"))
    {
      if (contextInfo.InputBindings["invocationContext"].Type == "signalRTrigger")
        return true;
    }

    if (contextInfo.InputBindings.ContainsKey("hostContext"))
    {
      if (contextInfo.InputBindings["hostContext"].Type == "signalRTrigger")
        return true;
    }

    return false;
  }

  public async Task Invoke(FunctionContext hostContext, FunctionExecutionDelegate next)
  {
    _logger.LogInformation("TTalkAuthMiddleware invoke");

    var contextInfo = hostContext.Items["contextHelper"] as ContextHelper;
    Guard.Argument(contextInfo).NotNull(nameof(contextInfo));

    // run the function
    await next(hostContext);
  }
}
