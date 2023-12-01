using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLabWebAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OLabWebAPI.Services;

public class OLabAuthMiddleware
{
  //private readonly IUserService _userService;
  //private readonly OLabDBContext _dbContext;
  private readonly RequestDelegate _next;
  private HttpRequest _httpRequest;

  private IReadOnlyDictionary<string, string> _headers;
  //private IReadOnlyDictionary<string, object> _bindingData;
  private string _functionName;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IServiceProvider serviceProvider,
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    RequestDelegate next)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("OLabAuthMiddleware created");

    _config = configuration;
    _next = next;
  }

  public static void SetupServices(
    IConfiguration configuration,
    IServiceCollection services,
    OLabDBContext dbContext)
  {
    var logger = new OLabLogger(NullLoggerFactory.Instance);
    // since IOLabconfiguration and OLabAuthentication can't be
    // injected into this method, we need to spin up temporary ones
    // so we can extract the centralized TokenValidationParameters
    // from IOLabAuthentication
    var config = new OLabConfiguration(NullLoggerFactory.Instance, configuration);
    var authentication = new OLabAuthentication(
      logger,
      config,
      dbContext);

    services.AddAuthentication(x =>
    {
      x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = true;
      options.SaveToken = true;
      options.TokenValidationParameters = authentication.GetValidationParameters();
      options.Events = new JwtBearerEvents
      {
        // this event fires on every failed token validation
        OnAuthenticationFailed = context =>
        {
          return Task.CompletedTask;
        },

        // this event fires on every incoming message
        OnMessageReceived = context =>
        {
          // If the request is for our SignalR hub based on
          // the URL requested then don't bother adding olab issued token.
          // SignalR has it's own
          var path = context.HttpContext.Request.Path;

          var accessToken = authentication.ExtractAccessToken(
            context.Request,
            path.Value.Contains("/login"));

          context.Token = accessToken;

          return Task.CompletedTask;
        }
      };
    });
  }

  public async Task InvokeAsync(
    HttpContext hostContext,
    OLabDBContext dbContext,
    IOLabAuthentication authentication)
  {
    Guard.Argument(hostContext).NotNull(nameof(hostContext));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(authentication).NotNull(nameof(authentication));

    try
    {
      _headers = hostContext.GetHttpRequestHeaders();
      _functionName = hostContext.Request.Path.HasValue ?
          Path.GetFileName(hostContext.Request.Path.Value) :
          string.Empty;

      _httpRequest = hostContext.GetHttpRequest();

      _logger.LogInformation($"Middleware Invoke. function '{_functionName}'");

      if (_functionName.ToLower().Contains("login") || _functionName.ToLower().Contains("health"))
        await _next(hostContext);

      // if not login endpoint, then continue with middleware evaluation
      else if (!_functionName.Contains("login"))
      {
        try
        {
          var token = authentication.ExtractAccessToken(_headers);

          authentication.ValidateToken(token);

          // This is added pre-function execution, function will have access to this information
          hostContext.Items.Add("headers", _headers);
          hostContext.Items.Add("claims", authentication.Claims);

          // build and inject the host context into the authorixation object
          var userContext = new UserContextService(_logger, hostContext);
          hostContext.Items.Add("usercontext", userContext);

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