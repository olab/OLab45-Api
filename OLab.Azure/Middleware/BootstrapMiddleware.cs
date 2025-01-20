using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Common.Interfaces;
using OLab.Api.Model;
using OLab.Azure.Utils;
using System.Threading;
using DocumentFormat.OpenXml.InkML;
using OLab.Common.Utils;
using OLab.Api.Utils;

namespace OLab.Azure.Middleware;
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
    var contextInfo = new ContextHelper( executionContext, _logger );
    Guard.Argument( contextInfo ).NotNull( nameof( contextInfo ) );

    executionContext.Items.Add( "contextHelper", contextInfo );

    await next( executionContext );
  }
}
