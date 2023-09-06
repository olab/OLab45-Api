using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OLab.FunctionApp.Services;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : JWTMiddleware
{
  private IUserService _userService;
  private OLabDBContext _dbContext;
  private IReadOnlyDictionary<string, string> _headers;
  private IReadOnlyDictionary<string, object> _bindingData;
  private string _functionName;
  private HttpRequestData _httpRequestData;

  public OLabAuthMiddleware(
    IConfiguration config,
    ILoggerFactory loggerFactory,
    IUserService userService,
    OLabDBContext dbContext) : base(config)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(userService).NotNull(nameof(userService));

    Logger = new OLabLogger(loggerFactory.CreateLogger<OLabAuthMiddleware>());
    _userService = userService;
    _dbContext = dbContext;
  }

  public override async Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
  {
    Guard.Argument(functionContext).NotNull(nameof(functionContext));
    Guard.Argument(next).NotNull(nameof(next));

    try
    {
      _headers = functionContext.GetHttpRequestHeaders();
      _bindingData = functionContext.BindingContext.BindingData;
      _functionName = functionContext.FunctionDefinition.Name.ToLower();
      _httpRequestData = functionContext.GetHttpRequestData();

      Logger.LogInformation($"Middleware executing for function '{_functionName}'");

      // if not login endpoint, then continue with middleware evaluation
      if (!_functionName.Contains("login"))
      {
        // This is added pre-function execution, function will have access to this information
        // in the context.Items dictionary
        functionContext.Items.Add("middlewareitem", "Hello, from middleware");

        try
        {
          var token = ExtractAccessToken(functionContext, true);
          var claimsPrincipal = ValidateToken(functionContext, token);

          functionContext.Items.Add("headers", _headers);

          // convert and save claims collection to dictionary
          var claimsDictionary = new Dictionary<string, string>();
          foreach (var claim in claimsPrincipal.Claims)
            claimsDictionary.Add(claim.Type, claim.Value);
          functionContext.Items.Add("claims", claimsDictionary);

          var auth = new OLabAuthorization(Logger, _dbContext, functionContext);
          functionContext.Items.Add("auth", auth);

        }
        catch (OLabUnauthorizedException)
        {
          // Unable to get token from headers
          await functionContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
          Logger.LogInformation("token not provided in request");
          return;
        }

      }
    }
    catch (Exception ex)
    {
      await functionContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      Logger.LogError($"server error: {ex.Message} {ex.StackTrace}");
      return;
    }

    // run the function
    await next(functionContext);

    // This happens after function execution. We can inspect the context after the function
    // was invoked
    if (functionContext.Items.TryGetValue("functionitem", out var value) && value is string message)
    {
      Logger.LogInformation($"From function: {message}");
    }
  }

  private ClaimsPrincipal ValidateToken(FunctionContext context, string token)
  {
    try
    {
      Guard.Argument(context).NotNull(nameof(context));

      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      var claimsPrincipal = tokenHandler.ValidateToken(
        token,
        TokenValidation,
        out SecurityToken validatedToken);

      return claimsPrincipal;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
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
  private string ExtractAccessToken(FunctionContext context, bool allowAnonymous = false)
  {
    Guard.Argument(context).NotNull(nameof(context));

    string token = string.Empty;

    if (_headers.TryGetValue("authorization", out var authHeader))
    {
      if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      {
        token = authHeader.Substring("Bearer ".Length).Trim();
        Logger.LogInformation("bearer token provided");
      }
    }

    // handler for external logins
    else if (_bindingData.TryGetValue("token", out var externalToken))
    {
      token = externalToken as string;
      Logger.LogInformation("external token provided");
    }

    // handler for signalR logins 
    else if (_bindingData.TryGetValue("access_token", out var signalRToken))
    {
      token = signalRToken as string;
      Logger.LogInformation("signalr token provided");
    }

    if (string.IsNullOrEmpty(token) && !allowAnonymous)
      throw new OLabUnauthorizedException();

    return token;
  }

}