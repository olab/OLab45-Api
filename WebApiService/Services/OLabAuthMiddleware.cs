using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using OLab.Access.Interfaces;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Services;
using OLabWebAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Services;

public class OLabAuthMiddleware
{
  private readonly IUserService _userService;
  private readonly OLabDBContext _dbContext;
  private readonly RequestDelegate _next;
  private HttpRequest _httpRequest;

  private IReadOnlyDictionary<string, string> _headers;
  //private IReadOnlyDictionary<string, object> _bindingData;
  private string _functionName;
  public IOLabAuthentication _authentication { get; private set; }
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IUserService userService,
    OLabDBContext dbContext,
    IOLabAuthentication authentication,
    RequestDelegate next)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(authentication).NotNull(nameof(authentication));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("OLabAuthMiddleware created");

    _config = configuration;
    _userService = userService;
    _dbContext = dbContext;
    _authentication = authentication; // new OLabAuthentication(loggerFactory, _config);
    _next = next;
  }

  public async Task InvokeAsync(HttpContext hostContext)
  {
    Guard.Argument(hostContext).NotNull(nameof(hostContext));

    try
    {
      _headers = hostContext.GetHttpRequestHeaders();
      _functionName = hostContext.Request.Method.ToLower();
      _httpRequest = hostContext.GetHttpRequest();

      _logger.LogInformation($"Middleware Invoke. function '{_functionName}'");

      if (_functionName.ToLower().Contains("login") || _functionName.ToLower().Contains("health"))
        await _next(hostContext);

      // if not login endpoint, then continue with middleware evaluation
      else if (!_functionName.Contains("login"))
      {
        try
        {
          if (!_headers.TryGetValue("authorization", out string token))
            throw new OLabUnauthorizedException();

          _authentication.ValidateToken(token);

          // This is added pre-function execution, function will have access to this information
          hostContext.Items.Add("headers", _headers);
          hostContext.Items.Add("claims", _authentication.Claims);

          var userContext = new UserContext(_logger, _dbContext, hostContext);
          var auth = new OLabAuthorization(_logger, _dbContext, userContext);
          hostContext.Items.Add("auth", auth);

          // run the function
          await _next(hostContext);

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