using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Common.Interfaces;
using System.Text.Json;
using System.Collections.Generic;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Utils;

public class ContextHelper
{
  public string FunctionName { get; private set; }
  public bool BypassMiddleware { get; private set; }

  public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public HttpRequestData RequestData { get; private set; }

  private readonly FunctionContext hostContext;
  private readonly IOLabLogger _logger;

  public ContextHelper(FunctionContext hostContext, IOLabLogger logger)
  {
    FunctionName = hostContext.FunctionDefinition.Name.ToLower();
    Guard.Argument(FunctionName).NotEmpty(nameof(FunctionName));

    this.hostContext = hostContext;
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
    if (hostContext.FunctionDefinition.InputBindings.ContainsKey("invocationContext"))
    {
      if (hostContext.FunctionDefinition.InputBindings["invocationContext"].Type == "signalRTrigger")
      {
        _logger.LogInformation("middleware bypass: turktalk");
        return true;
      }
    }

    if (hostContext.FunctionDefinition.InputBindings.ContainsKey("hostContext"))
    {
      if (hostContext.FunctionDefinition.InputBindings["hostContext"].Type == "signalRTrigger")
      {
        _logger.LogInformation("middleware bypass: turktalk");
        return true;
      }
    }

    if (FunctionName.ToLower().Contains("login") ||
        FunctionName.ToLower().Contains("health") ||
        FunctionName.ToLower().Contains("index")
        //FunctionName.ToLower().Contains("negotiate")
    )
    {
      _logger.LogInformation("middleware bypass: url");
      return true;
    }

    // hostContext.FunctionDefinition.InputBindings["invocationContext"].Type == "signalRTrigger")

    _logger.LogInformation("middleware active");
    return false;
  }

}