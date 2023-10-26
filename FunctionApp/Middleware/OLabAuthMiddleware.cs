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
using OLab.Data.Interface;
using OLab.Api.Data.Interface;

namespace OLab.FunctionApp.Middleware;

public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly OLabDBContext _dbContext;
  private readonly IOLabAuthorization _authorization;
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
    OLabDBContext dbContext,
    IOLabAuthentication authentication,
    IOLabAuthorization authorization)
  {
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(authentication).NotNull(nameof(authentication));
    Guard.Argument(authorization).NotNull(nameof(authorization));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("JwtMiddleware created");

    _config = configuration;
    _dbContext = dbContext;
    _authentication = authentication; // new OLabAuthentication(loggerFactory, _config);
    _authorization = authorization;
  }

  public async Task Invoke(
    FunctionContext hostContext, 
    FunctionExecutionDelegate next)
  {
    Guard.Argument(hostContext).NotNull(nameof(hostContext));
    Guard.Argument(next).NotNull(nameof(next));

    try
    {
      _headers = hostContext.GetHttpRequestHeaders();
      _bindingData = hostContext.BindingContext.BindingData;
      _functionName = hostContext.FunctionDefinition.Name.ToLower();
      _httpRequestData = hostContext.GetHttpRequestData();

      _logger.LogInformation($"Middleware Invoke. function '{_functionName}'");

      // skip middleware for non-authenicated endpoints
      if (_functionName.ToLower().Contains("login") || _functionName.ToLower().Contains("health"))
        await next(hostContext);

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
          hostContext.Items.Add("headers", _headers);
          hostContext.Items.Add("claims", _authentication.Claims);

          var userContext = new UserContext(_logger, _dbContext, hostContext);
          _authorization.SetUserContext( userContext );
          hostContext.Items.Add("auth", _authorization);

          // run the function
          await next(hostContext);

          // This happens after function execution. We can inspect the context after the function
          // was invoked
          if (hostContext.Items.TryGetValue("functionitem", out var value) && value is string message)
          {
            _logger.LogInformation($"From function: {message}");
          }

        }
        catch (OLabUnauthorizedException)
        {
          // Unable to get token from headers
          await hostContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
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
      await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      _logger.LogError($"server error: {ex.Message} {ex.StackTrace}");
      return;
    }

  }

}