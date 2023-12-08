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
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OLab.Data.Interface;
using OLab.Api.Data.Interface;
using DocumentFormat.OpenXml.InkML;
using System.Text.Json;
using OLab.Data.BusinessObjects.API;
using DocumentFormat.OpenXml.Math;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using IsolatedModel_BidirectionChat.Extensions;
using IsolatedModel_BidirectionChat.Services;

namespace IsolatedModel_BidirectionChat.Middleware;

public class ContextInformation
{
  public string FunctionName { get; private set; }
  public bool BypassMiddleware { get; private set; }

  public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public HttpRequestData RequestData { get; private set; }

  private readonly IOLabLogger _logger;

  public ContextInformation(FunctionContext hostContext, IOLabLogger logger)
  {
    FunctionName = hostContext.FunctionDefinition.Name.ToLower();
    Guard.Argument(FunctionName).NotEmpty(nameof(FunctionName));

    _logger = logger;

    _logger.LogInformation($"ContextInformation");
    _logger.LogInformation($"  function name: {FunctionName}");

    Headers = hostContext.GetHttpRequestHeaders();
    Guard.Argument(Headers).NotNull(nameof(Headers));

    foreach (var header in Headers)
      logger.LogInformation($"  header: {header.Key} = {header.Value}");

    BindingData = hostContext.BindingContext.BindingData;
    Guard.Argument(BindingData).NotNull(nameof(BindingData));

    _logger.LogInformation($"  binding context: {JsonSerializer.Serialize(hostContext.BindingContext)}");

    foreach (var inputBinding in hostContext.FunctionDefinition.InputBindings)
      _logger.LogInformation($"  input binding: {inputBinding.Key} = {inputBinding.Value.Name}({inputBinding.Value.Type})");

    RequestData = hostContext.GetHttpRequestData();
    if (RequestData != null)
      _logger.LogInformation($"  url: {RequestData.Url}");

    BypassMiddleware = EvaluateHostContext();
  }

  private bool EvaluateHostContext()
  {
    return true;

    if (FunctionName.ToLower().Contains("login") ||
        FunctionName.ToLower().Contains("health") ||
        FunctionName.ToLower().Contains("index") ||
        FunctionName.ToLower().Contains("testpage") ||
        FunctionName.ToLower().Contains("negotiate"))
    {
      _logger.LogInformation("middleware bypass: url");
      return true;
    }

    // hostContext.FunctionDefinition.InputBindings["invocationContext"].Type == "signalRTrigger")

    _logger.LogInformation("middleware active");
    return false;
  }
}

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

      var contextInfo = new ContextInformation(hostContext, _logger);

      // test for non-authenicated endpoints
      if (contextInfo.BypassMiddleware)
        await next(hostContext);

      // else is auth endpoint, then continue with middleware evaluation
      else
        try
        {
          _logger.LogInformation("evaluating REST API method");

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