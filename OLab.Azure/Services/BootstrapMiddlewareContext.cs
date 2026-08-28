using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OLab.Azure.Services;

/// <summary>
/// Helper class to manage and extract information
/// from the Azure Function execution context.
/// </summary>
public class BootstrapMiddlewareContext
{
  public string FunctionName { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public IReadOnlyDictionary<string, BindingMetadata> InputBindings { get; private set; }
  public HttpRequestData Request { get; private set; }
  public string Url { get; private set; }
  public FunctionContext ExecutionContext { get; }
  public IDictionary<string, string> Headers { get; private set; }

  private readonly IOLabLogger _logger;
  private IOLabLogger GetLogger() { return _logger; }

  public static BootstrapMiddlewareContext CreateInjectInstance(FunctionContext executionContext, IOLabLogger logger)
  {
    var context = new BootstrapMiddlewareContext( executionContext, logger );
    executionContext.Items.Add( context.GetType().Name, context );
    return context;
  }

  public BootstrapMiddlewareContext(FunctionContext executionContext, IOLabLogger logger)
  {
    ExecutionContext = executionContext;
    _logger = logger;

    try
    {
      GetLogger().LogInformation( "BootstrapMiddlewareContext ctor" );

      FunctionName = executionContext.FunctionDefinition.Name.ToLower();
      Guard.Argument( FunctionName ).NotEmpty( nameof( FunctionName ) );

      GetLogger().LogInformation( $"  function name: {FunctionName}" );

      var httpRequestData = executionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
      Guard.Argument( httpRequestData ).NotNull( nameof( httpRequestData ) );

      Request = httpRequestData;

      InputBindings = executionContext.FunctionDefinition.InputBindings;

      Headers = ExtractHeaders( httpRequestData );

      BindingData = executionContext.BindingContext.BindingData;
      Guard.Argument( BindingData ).NotNull( nameof( BindingData ) );

      Url = httpRequestData.Url.ToString();
      GetLogger().LogInformation( $"  url: {Url}" );
    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex, "BootstrapMiddlewareContext exception" );
      throw;
    }
  }

  /// <summary>
  /// Extracts headers from the given HttpRequestData and returns them as a dictionary.
  /// </summary>
  private IDictionary<string, string> ExtractHeaders(HttpRequestData httpRequestData)
  {
    var flatHeaderDict = new Dictionary<string, string>();

    foreach ( var header in httpRequestData.Headers )
      flatHeaderDict.Add( header.Key.ToLower(), header.Value.First() );

    return flatHeaderDict;
  }

  /// <summary>
  /// Retrieves the value of a specified header from the request headers.
  /// </summary>
  public string GetHeader(string key, bool isRequired = true)
  {
    if ( Headers.TryGetValue( key.ToLower(), out var value ) )
      return value;

    if ( isRequired )
      throw new Exception( $"header value '{key}' does not exist" );

    return string.Empty;
  }

  public override string ToString()
  {
    return $"{FunctionName}";
  }
}
