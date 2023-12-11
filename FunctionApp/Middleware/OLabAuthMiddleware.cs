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

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private OLabDBContext _dbContext;

  public IOLabAuthentication _authentication { get; private set; }
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IOLabAuthentication authentication)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(authentication).NotNull(nameof(authentication));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("JwtMiddleware created");

    _config = configuration;
    _authentication = authentication;
  }

  public async Task Invoke(
    FunctionContext hostContext,
    FunctionExecutionDelegate next)
  {
    Guard.Argument(hostContext).NotNull(nameof(hostContext));
    Guard.Argument(next).NotNull(nameof(next));

    try
    {
      Guard.Argument(hostContext).NotNull(nameof(hostContext));

      var contextInfo = new ContextHelper(hostContext, _logger);

      var dbContext = hostContext.InstanceServices.GetService(typeof(OLabDBContext)) as OLabDBContext;
      Guard.Argument(dbContext).NotNull(nameof(dbContext));
      _dbContext = dbContext;

      // test for non-authenicated endpoints
      if (contextInfo.BypassMiddleware)
        await next(hostContext);

      // else is auth endpoint, then continue with middleware evaluation
      else
        try
        {
          _logger.LogInformation("evaluating REST API method");

          var token = _authentication.ExtractAccessToken(contextInfo.Headers, contextInfo.BindingData);

          _authentication.ValidateToken(token);

          // these must be set before building UserContextService 
          hostContext.Items.Add("headers", contextInfo.Headers);
          hostContext.Items.Add("claims", _authentication.Claims);

          // This is added pre-function execution, function will have access to this information
          var userContext = new UserContextService(_logger, hostContext);
          hostContext.Items.Add("usercontext", userContext);

          // run the function
          await next(hostContext);

          // This happens after function execution. We can inspect the context after the function
          // was invoked
          if (hostContext.Items.TryGetValue("functionitem", out var value) && value is string message)
            _logger.LogInformation($"From function: {message}");

        }
        catch (OLabUnauthorizedException ex)
        {
          _logger.LogError($"function auth error: {ex.Message} {ex.StackTrace}");
          // Unable to get token from headers
          await hostContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
        }
        catch (Exception ex)
        {
          _logger.LogError($"function error: {ex.Message} {ex.StackTrace}");
          await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
        }
      //else
      //  _logger.LogWarning($"Unknown HTTP request {_functionName}");
    }
    catch (Exception ex)
    {
      _logger.LogError($"Middleware error: {ex.Message} {ex.StackTrace}");
      await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
    }

  }

}