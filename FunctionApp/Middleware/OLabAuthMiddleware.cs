
using Api;
using Azure.Core;
using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OLab.FunctionApp.Functions;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : JWTMiddleware
{
  private IUserService _userService;

  public OLabAuthMiddleware(IConfiguration config, ILoggerFactory loggerFactory, IUserService userService) : base(config)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(userService).NotNull(nameof(userService));

    Logger = new OLabLogger(loggerFactory.CreateLogger<OLabAuthMiddleware>());
    _userService = userService;
  }

  public override async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    Guard.Argument(context).NotNull(nameof(context));
    Guard.Argument(next).NotNull(nameof(next));

    Logger.LogInformation($"Middleware executing for function '{context.FunctionDefinition.Name}'");

    // if not login endpoint, then continue with middleware evaluation
    if (!context.FunctionDefinition.Name.ToLower().Contains("login"))
    {
      // This is added pre-function execution, function will have access to this information
      // in the context.Items dictionary
      context.Items.Add("middlewareitem", "Hello, from middleware");

      string token = string.Empty;

      try
      {
        token = ExtractAccessToken(context, true);

        (Type featureType, object featureInstance) = context.Features.SingleOrDefault(x => x.Key.Name == "IFunctionBindingsFeature");

        // find the input binding of the function which has been invoked and then find the associated parameter of the function for the data we want
        var inputData = featureType.GetProperties().SingleOrDefault(p => p.Name == "InputData")?.GetValue(featureInstance) as IReadOnlyDictionary<string, object>;
        var requestData = inputData?.Values.SingleOrDefault(obj => obj is HttpRequestData) as HttpRequestData;

        if (requestData?.ParsePrincipal() is ClaimsPrincipal principal)
        {
          // set the principal on the accessor from DI
          var accessor = context.InstanceServices.GetRequiredService<IClaimsPrincipalAccessor>();
          accessor.Principal = principal;
        }

        ValidateToken(context, token);

      }
      catch (OLabUnauthorizedException)
      {
        // Unable to get token from headers
        await context.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
        Logger.LogInformation("Could not get token from request");
        return;
      }

      // TODO: validate token

    }

    await next(context);

    // This happens after function execution. We can inspect the context after the function
    // was invoked
    if (context.Items.TryGetValue("functionitem", out var value) && value is string message)
    {
      Logger.LogInformation($"From function: {message}");
    }
  }

  private void ValidateToken(FunctionContext context, string token)
  {
    try
    {
      Guard.Argument(context).NotNull(nameof(context));

      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken(token,
                                 TokenValidation,
                                 out SecurityToken validatedToken);
    }
    catch (Exception ex)
    {
      Logger.LogException(ex);
      throw new OLabUnauthorizedException();
    }
  }

  /// <summary>
  /// Gets the access token from the request
  /// </summary>
  /// <param name="logger">ILogger instance</param>
  /// <param name="context">Function context</param>
  /// <param name="token">(out) JWT token</param>
  /// <returns>true if token found</returns>
  private static string ExtractAccessToken(FunctionContext context, bool allowAnonymous = false)
  {
    Guard.Argument(context).NotNull(nameof(context));

    var headers = context.GetHttpRequestHeaders();

    string token = string.Empty;

    if (headers.TryGetValue("authorization", out var authHeaderValue))
    {
      if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      {
        token = authHeaderValue.Substring("Bearer ".Length).Trim();
        Logger.LogInformation("bearer token provided");
      }
    }

    // handler for external logins
    else if (context.BindingContext.BindingData.TryGetValue("token", out var externalToken))
    {
      token = externalToken as string;
      Logger.LogInformation("external token provided");
    }

    // handler for signalR logins 
    else if (context.BindingContext.BindingData.TryGetValue("access_token", out var signalRToken))
    {
      token = signalRToken as string;
      Logger.LogInformation("signalr token provided");
    }

    if (string.IsNullOrEmpty(token) && !allowAnonymous)
      throw new OLabUnauthorizedException();

    return token;
  }

}