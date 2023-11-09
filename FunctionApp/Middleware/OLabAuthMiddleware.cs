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
  private OLabDBContext _dbContext;
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
    IOLabAuthentication authentication)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
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
      var dbContext = hostContext.InstanceServices.GetService(typeof(OLabDBContext)) as OLabDBContext;
      Guard.Argument(dbContext).NotNull(nameof(dbContext));

      _dbContext = dbContext;

      _headers = hostContext.GetHttpRequestHeaders();
      _bindingData = hostContext.BindingContext.BindingData;
      _functionName = hostContext.FunctionDefinition.Name.ToLower();
      _httpRequestData = hostContext.GetHttpRequestData();

      _logger.LogInformation($"Middleware Invoke. function '{_functionName}'");
      foreach (var header in _headers)
        _logger.LogInformation($"  header: {header.Key} = {header.Value}");

      // skip middleware for non-authenicated endpoints
      if (_functionName.ToLower().Contains("login") || _functionName.ToLower().Contains("health"))
        await next(hostContext);

      // if not login endpoint, then continue with middleware evaluation
      else if (!_functionName.Contains("login"))
      {
        try
        {
          var token = _authentication.ExtractAccessToken(_headers, _bindingData);

          _authentication.ValidateToken(token);

          // these must be set before building UserContextService 
          hostContext.Items.Add("headers", _headers);
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
          throw;
        }
        catch (Exception ex)
        {
          _logger.LogError($"function error: {ex.Message} {ex.StackTrace}");
          await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
          throw;
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError($"Middleware error: {ex.Message} {ex.StackTrace}");
      await hostContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      throw;
    }

  }

}