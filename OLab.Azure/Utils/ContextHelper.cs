using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace OLab.Azure.Utils;

public class ContextHelper
{
  public string FunctionName { get; private set; }
  public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public IReadOnlyDictionary<string, BindingMetadata> InputBindings { get; private set; }
  public HttpRequest Request { get; private set; }
  public string Url { get; private set; }

  private readonly FunctionContext hostContext;
  private readonly IOLabLogger _logger;

  public ContextHelper(FunctionContext hostContext, IOLabLogger logger)
  {
    FunctionName = hostContext.FunctionDefinition.Name.ToLower();
    Guard.Argument( FunctionName ).NotEmpty( nameof( FunctionName ) );

    this.hostContext = hostContext;
    _logger = logger;

    _logger.LogInformation( $"ContextInformation:" );
    _logger.LogInformation( $"  function name: {FunctionName}" );

    var headerDict = hostContext.GetHttpRequestData().Headers.ToDictionary();
    var flatHeaderDict = new Dictionary<string, string>();
    foreach ( var header in headerDict )
      flatHeaderDict.Add( header.Key, header.Value.First() );
    Headers = flatHeaderDict;

    Guard.Argument( Headers ).NotNull( nameof( Headers ) );

    foreach ( var header in Headers )
      logger.LogInformation( $"  header: {header.Key} = {header.Value}" );

    BindingData = hostContext.BindingContext.BindingData;
    Guard.Argument( BindingData ).NotNull( nameof( BindingData ) );

    _logger.LogInformation( $"  binding context: {JsonSerializer.Serialize( hostContext.BindingContext ).Replace( "\u0022", "\"" )}" );

    InputBindings = hostContext.FunctionDefinition.InputBindings;
    foreach ( var inputBinding in InputBindings )
      _logger.LogInformation( $"  input binding: {inputBinding.Key} = {inputBinding.Value.Name}({inputBinding.Value.Type})" );

    var context = hostContext.Items[ "HttpRequestContext" ] as DefaultHttpContext;
    Request = context.Request;
    Url = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/{Request.Path}";

    _logger.LogInformation( $"  url: {Url}" );

  }

  public override string ToString()
  {
    return $"{FunctionName}";
  }

}