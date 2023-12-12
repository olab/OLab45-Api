using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Extensions;
using OLab.FunctionApp.Utils;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Middleware;

/// <summary>
/// Pre-authorization middleware
/// </summary>
public class OpenAuthMiddleware : IFunctionsWorkerMiddleware
{
  private IOLabLogger _logger;
  private IOLabConfiguration _config;

  public OpenAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("OpenAuthMiddleware created");

    _config = configuration;
  }

  public static bool CanInvoke(FunctionContext context)
  {
    return !context.GetHttpRequestHeaders().ContainsKey("authorization");
  }

  public async Task Invoke(FunctionContext hostContext, FunctionExecutionDelegate next)
  {
    _logger.LogInformation("OpenAuthMiddleware invoke");

    var contextInfo = hostContext.Items["contextHelper"] as ContextHelper;
    Guard.Argument(contextInfo).NotNull(nameof(contextInfo));

    await next(hostContext);
  }
}
