using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Azure.Services;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System.Threading.Tasks;


namespace OLab.Azure.Middleware;

/// <summary>
/// Middleware for exposing the execution context
/// </summary>
public class BootstrapMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabLogger _logger;

  public BootstrapMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory)
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "BootstrapMiddleware created" );

  }

  public async Task Invoke(FunctionContext executionContext, FunctionExecutionDelegate next)
  {
    BootstrapMiddlewareContext.CreateInjectInstance( executionContext, _logger );
    await next( executionContext );
  }
}
