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
  public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public IReadOnlyDictionary<string, BindingMetadata> InputBindings{ get; private set; }
  public HttpRequestData RequestData { get; private set; }

  private readonly FunctionContext hostContext;
  private readonly IOLabLogger _logger;

  public ContextHelper(FunctionContext hostContext, IOLabLogger logger)
  {
    FunctionName = hostContext.FunctionDefinition.Name.ToLower();
    Guard.Argument(FunctionName).NotEmpty(nameof(FunctionName));

    this.hostContext = hostContext;
    _logger = logger;

    _logger.LogInformation($"ContextInformation:");
    _logger.LogInformation($"  function name: {FunctionName}");

    Headers = hostContext.GetHttpRequestHeaders();
    Guard.Argument(Headers).NotNull(nameof(Headers));

    foreach (var header in Headers)
      logger.LogInformation($"  header: {header.Key} = {header.Value}");

    BindingData = hostContext.BindingContext.BindingData;
    Guard.Argument(BindingData).NotNull(nameof(BindingData));

    _logger.LogInformation($"  binding context: {JsonSerializer.Serialize(hostContext.BindingContext).Replace("\u0022", "\"")}");

    InputBindings = hostContext.FunctionDefinition.InputBindings;
    foreach (var inputBinding in InputBindings)
      _logger.LogInformation($"  input binding: {inputBinding.Key} = {inputBinding.Value.Name}({inputBinding.Value.Type})");

    RequestData = hostContext.GetHttpRequestData();
    if (RequestData != null)
      _logger.LogInformation($"  url: {RequestData.Url}");

  }

  public override string ToString()
  {
    return $"{FunctionName}";
  }

}