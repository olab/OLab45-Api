using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Api.Common.Exceptions;
using OLab.Api.Utils;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System;
using OLab.FunctionApp.Extensions;
using OLab.FunctionApp.Services;
using OLab.FunctionApp.Utils;
using OLab.Data.BusinessObjects;
using OLab.Access;
using System.Security.Cryptography.Pkcs;

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;
  private OLabDBContext _dbContext;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IOLabAuthentication authentication,
    OLabDBContext dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(authentication).NotNull(nameof(authentication));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("OLabAuthMiddleware created");

    _config = configuration;
    _dbContext = dbContext;
  }

  public static bool CanInvoke(FunctionContext context)
  {
    return context.GetHttpRequestHeaders().ContainsKey("authorization");
  }

  public async Task Invoke(
    FunctionContext hostContext,
    FunctionExecutionDelegate next)
  {
    Guard.Argument(hostContext).NotNull(nameof(hostContext));

    try
    {
      _logger.LogInformation("OLabAuthMiddleware invoke");

      var contextInfo = hostContext.Items["contextHelper"] as ContextHelper;
      Guard.Argument(contextInfo).NotNull(nameof(contextInfo));

      try
      {
        var authentication = new OLabAuthentication(_logger, _config, _dbContext);
        var token = authentication.ExtractAccessToken(contextInfo.Headers, contextInfo.BindingData);

        authentication.ValidateToken(token);

        // these must be set before building UserContextService 
        hostContext.Items.Add("headers", contextInfo.Headers);
        hostContext.Items.Add("claims", authentication.Claims);

        // This is added pre-function execution, function will have access to this information
        var userContext = new UserContextService(_logger, hostContext);
        hostContext.Items.Add("usercontext", userContext);

        // This happens after function execution. We can inspect the context after the function
        // was invoked
        //if (hostContext.Items.TryGetValue("functionitem", out var value) && value is string message)
        //  _logger.LogInformation($"From function: {message}");

      }
      catch (Exception ex)
      {
        _logger.LogError($"function error: {ex.Message} {ex.StackTrace}");
        await hostContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "could not process token." });
      }

      await next(hostContext);

    }
    catch (Exception ex)
    {
      _logger.LogError($"OLabAuthMiddleware error: {ex.Message} {ex.StackTrace}");
      await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
    }

  }

}