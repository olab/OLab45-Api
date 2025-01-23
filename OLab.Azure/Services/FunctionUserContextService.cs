using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;

#nullable disable

namespace OLab.Azure.Services;

public class FunctionUserContextService : UserContextService
{
  // default ctor, needed for services Dependancy Injection
  public FunctionUserContextService()
  {

  }

  public FunctionUserContextService(
    IOLabLogger logger,
    FunctionContext executionContext,
    OLabDBContext dbContext) : base( logger, dbContext )
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );
    Guard.Argument( executionContext ).NotNull( nameof( executionContext ) );

    GetLogger().LogInformation( $"UserContext ctor" );

    LoadHostContext( executionContext );
  }

  private string GetRequestIpAddress(HttpRequestData req)
  {
    try
    {
      var headerDictionary = req.Headers.ToDictionary( x => x.Key, x => x.Value, StringComparer.Ordinal );
      var key = "x-forwarded-for";

      if ( headerDictionary.ContainsKey( key ) )
      {
        var headerValues = headerDictionary[ key ];
        var ipn = headerValues?.FirstOrDefault()?.Split( new char[] { ',' } ).FirstOrDefault()?.Split( new char[] { ':' } ).FirstOrDefault();

        GetLogger().LogInformation( $"found ip address: {ipn}" );

        return ipn;
      }

    }
    catch ( Exception )
    {
      // eat all exceptions
    }

    return "<unknown>";
  }

  protected void LoadHostContext(FunctionContext executionContext)
  {
    var req = executionContext.GetHttpRequestData();
    IPAddress = GetRequestIpAddress( req );

    if ( !executionContext.Items.TryGetValue( "headers", out var headersObjects ) )
      throw new Exception( "unable to retrieve headers from host context" );
    SetHeaders( (Dictionary<string, string>)headersObjects );

    if ( !executionContext.Items.TryGetValue( "claims", out var claimsObject ) )
      throw new Exception( "unable to retrieve claims from host context" );
    SetClaims( (IDictionary<string, string>)claimsObject );

    LoadContext();

  }

}

