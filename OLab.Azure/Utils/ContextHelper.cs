using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Common.Interfaces;

namespace OLab.Azure.Utils;

public class ContextHelper
{
  public string FunctionName { get; private set; }
  public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public IReadOnlyDictionary<string, BindingMetadata> InputBindings { get; private set; }
  public HttpRequest Request { get; private set; }
  public string Url { get; private set; }

  private readonly FunctionContext executionContext;
  private readonly IOLabLogger _logger;

  public ContextHelper(FunctionContext executionContext, IOLabLogger logger)
  {
    this.executionContext = executionContext;
    _logger = logger;

    try
    {
      _logger.LogInformation( $"ContextHelper ctor" );

      FunctionName = executionContext.FunctionDefinition.Name.ToLower();
      Guard.Argument( FunctionName ).NotEmpty( nameof( FunctionName ) );

      _logger.LogInformation( $"  function name: {FunctionName}" );

      var httpRequestData = executionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();

      Headers = ExtractHeaders( httpRequestData );

      BindingData = executionContext.BindingContext.BindingData;
      Guard.Argument( BindingData ).NotNull( nameof( BindingData ) );

      InputBindings = executionContext.FunctionDefinition.InputBindings;

      var context = executionContext.Items[ "HttpRequestContext" ] as DefaultHttpContext;
      Request = context.Request;

      Url = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/{Request.Path}";
      _logger.LogInformation( $"  url: {Url}" );

    }
    catch ( Exception ex )
    {
      _logger.LogError( ex, "ContextHelper exception" );
      throw;
    }

  }

  /// <summary>
  /// Extracts headers from the given HttpRequestData and returns them as a dictionary.
  /// </summary>
  /// <param name="httpRequestData">The HttpRequestData containing the headers to extract.</param>
  /// <returns>A dictionary containing the headers as key-value pairs.</returns>
  private Dictionary<string, string> ExtractHeaders(HttpRequestData httpRequestData)
  {
    var flatHeaderDict = new Dictionary<string, string>();
    foreach ( var header in httpRequestData.Headers )
      flatHeaderDict.Add( header.Key, header.Value.First() );

    foreach ( var header in flatHeaderDict )
    {
      _logger.LogInformation( $"  header: {header.Key} = {header.Value}" );
      Console.WriteLine( $"  write header: {header.Key} = {header.Value}" );
    }

    return flatHeaderDict;
  }

  public override string ToString()
  {
    return $"{FunctionName}";
  }

}