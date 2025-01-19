using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System.Threading.Tasks;

namespace OLab.Azure.Middleware;

/// <summary>
/// Pre-authorization middleware
/// </summary>
public class OpenAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabLogger _logger;
  private readonly IOLabConfiguration _config;

  public OpenAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory)
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "OpenAuthMiddleware created" );

    _config = configuration;
  }

  public static bool CanInvoke(FunctionContext context)
  {
    var requestData = context.GetHttpResponseData();
    return !requestData.Headers.Contains( "authorization" );
  }

  public async Task Invoke(FunctionContext executionContext, FunctionExecutionDelegate next)
  {
    _logger.LogInformation( "OpenAuthMiddleware invoke" );

    var contextInfo = executionContext.Items[ "contextHelper" ] as ContextHelper;
    Guard.Argument( contextInfo ).NotNull( nameof( contextInfo ) );

    await next( executionContext );
  }
}
