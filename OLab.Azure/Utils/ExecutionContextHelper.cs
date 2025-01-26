using Dawn;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Common.Interfaces;

namespace OLab.Azure.Utils;

public class ExecutionContextHelper
{
  public string FunctionName { get; private set; }
  //public IReadOnlyDictionary<string, string> Headers { get; private set; }
  public IReadOnlyDictionary<string, object> BindingData { get; private set; }
  public IReadOnlyDictionary<string, BindingMetadata> InputBindings { get; private set; }
  public HttpRequest Request { get; private set; }
  public string Url { get; private set; }
  public FunctionContext ExecutionContext { get; }
  public IDictionary<string, string> Headers { get; private set; }

  private readonly IOLabLogger _logger;

  public ExecutionContextHelper(FunctionContext executionContext, IOLabLogger logger)
  {
    ExecutionContext = executionContext;
    _logger = logger;

    try
    {
      _logger.LogInformation( $"ExecutionContextHelper ctor" );

      FunctionName = executionContext.FunctionDefinition.Name.ToLower();
      Guard.Argument( FunctionName ).NotEmpty( nameof( FunctionName ) );

      _logger.LogInformation( $"  function name: {FunctionName}" );

      var httpRequestData = executionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();

      InputBindings = executionContext.FunctionDefinition.InputBindings;
      Headers = ExtractHeaders( httpRequestData );
      _logger.LogInformation( $"found {Headers.Count} headers" );

      BindingData = executionContext.BindingContext.BindingData;
      Guard.Argument( BindingData ).NotNull( nameof( BindingData ) );

      var context = executionContext.Items[ "HttpRequestContext" ] as DefaultHttpContext;
      Request = context.Request;

      Url = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/{Request.Path}";
      _logger.LogInformation( $"  url: {Url}" );

    }
    catch ( Exception ex )
    {
      _logger.LogError( ex, "ExecutionContextHelper exception" );
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
      flatHeaderDict.Add( header.Key.ToLower(), header.Value.First() );

    //foreach ( var header in flatHeaderDict )
    //  _logger.LogInformation( $"  header: {header.Key} = {header.Value}" );

    return flatHeaderDict;
  }

  protected string GetHeader(string key, bool isRequired = true)
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