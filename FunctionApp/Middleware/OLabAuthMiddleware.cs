using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Logging;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Services;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IUserService _userService;
  private readonly OLabDBContext _dbContext;
  private HttpRequestData _httpRequestData;

  private IReadOnlyDictionary<string, string> _headers;
  private IReadOnlyDictionary<string, object> _bindingData;
  private string _functionName;
  public IOLabAuthentication _authentication { get; private set; }
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IUserService userService,
    OLabDBContext dbContext,
    IOLabAuthentication authentication)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(authentication).NotNull(nameof(authentication));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("JwtMiddleware created");

    _config = configuration;
    _userService = userService;
    _dbContext = dbContext;
    _authentication = authentication; // new OLabAuthentication(loggerFactory, _config);

  }

  public async Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
  {
    Guard.Argument(functionContext).NotNull(nameof(functionContext));
    Guard.Argument(next).NotNull(nameof(next));

    try
    {
      _headers = functionContext.GetHttpRequestHeaders();
      _bindingData = functionContext.BindingContext.BindingData;
      _functionName = functionContext.FunctionDefinition.Name.ToLower();
      _httpRequestData = functionContext.GetHttpRequestData();

      _logger.LogInformation($"Middleware Invoke. function '{_functionName}'");

      // skip middleware for non-authenicated endpoints
      if (_functionName.ToLower().Contains("login") || _functionName.ToLower().Contains("health"))
        await next(functionContext);

      // if not login endpoint, then continue with middleware evaluation
      else if (!_functionName.Contains("login"))
      {
        try
        {
          var token = _authentication.ExtractAccessToken(_headers, _bindingData);

          if (string.IsNullOrEmpty(token))
            throw new OLabUnauthorizedException();

          _authentication.ValidateToken(token);

          // This is added pre-function execution, function will have access to this information
          functionContext.Items.Add("headers", _headers);
          functionContext.Items.Add("claims", _authentication.Claims);

          var userContext = new UserContext(_logger, _dbContext, functionContext);
          var auth = new OLabAuthorization(_logger, _dbContext, userContext);
          functionContext.Items.Add("auth", auth);

          // run the function
          await next(functionContext);

          // This happens after function execution. We can inspect the context after the function
          // was invoked
          if (functionContext.Items.TryGetValue("functionitem", out var value) && value is string message)
          {
            _logger.LogInformation($"From function: {message}");
          }

        }
        catch (OLabUnauthorizedException)
        {
          // Unable to get token from headers
          await functionContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
          _logger.LogInformation("token not provided in request");
          return;
        }
        catch (Exception ex)
        {
          _logger.LogError($"function error: {ex.Message} {ex.StackTrace}");
          return;
        }
      }
    }
    catch (Exception ex)
    {
      await functionContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      _logger.LogError($"server error: {ex.Message} {ex.StackTrace}");
      return;
    }

  }

}