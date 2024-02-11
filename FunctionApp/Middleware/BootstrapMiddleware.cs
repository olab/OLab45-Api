using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Model;
using OLab.FunctionApp.Utils;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Middleware;
public class BootstrapMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;

  public BootstrapMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    OLabDBContext dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("BootstrapMiddleware created");

    _config = configuration;
    _dbContext = dbContext;
  }

  public async Task Invoke(FunctionContext hostContext, FunctionExecutionDelegate next)
  {
    var contextInfo = new ContextHelper(hostContext, _logger);
    Guard.Argument(contextInfo).NotNull(nameof(contextInfo));

    hostContext.Items.Add("contextHelper", contextInfo);

    await next(hostContext);
  }
}
