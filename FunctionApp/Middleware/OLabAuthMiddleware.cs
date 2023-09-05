
using Azure.Core;
using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OLab.FunctionApp.Middleware
{
  public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
  {
    public OLabAuthMiddleware()
    {

    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
      Guard.Argument(context).NotNull(nameof(context));
      Guard.Argument(next).NotNull(nameof(next));

      ILogger logger = context.GetLogger<OLabAuthMiddleware>();
      logger.LogInformation("Middleware executing for function '{name}'", context.FunctionDefinition.Name);

      // if not login endpoint, then continue with middleware
      if (!context.FunctionDefinition.Name.Contains("login"))
      {
        // This is added pre-function execution, function will have access to this information
        // in the context.Items dictionary
        // context.Items.Add("middlewareitem", "Hello, from middleware");

        if (!GetToken(logger, context, out var authHeaderValue))
        {
          // Unable to get token from headers
          await context.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
          logger.LogInformation("Could not get token from header");
          return;
        }

        // TODO: validate token

      }

      await next(context);

      // This happens after function execution. We can inspect the context after the function
      // was invoked
      if (context.Items.TryGetValue("functionitem", out var value) && value is string message)
      {
        logger.LogInformation("From function: {message}", message);
      }
    }

    private static bool GetToken(ILogger logger, FunctionContext context, out string token)
    {
      var headers = context.GetHttpRequestHeaders();

      token = null;

      if (headers.TryGetValue("authorization", out var authHeaderValue))
      {

        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
          return false;

        token = authHeaderValue.Substring("Bearer ".Length).Trim();
        logger.LogInformation("got bearer token");

        return true;
      }

      // handler for external logins
      if (context.BindingContext.BindingData.TryGetValue("token", out var externalToken))
      {
        token = externalToken as string;
        logger.LogInformation("got external login token");
        return true;
      }

      // handler for signalR logins 
      if (context.BindingContext.BindingData.TryGetValue("access_token", out var signalRToken))
      {
        token = signalRToken as string;
        logger.LogInformation("got signalr token");
        return true;
      }

      return false;
    }

  }
}