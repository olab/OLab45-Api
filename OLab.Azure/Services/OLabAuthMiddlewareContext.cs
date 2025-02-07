using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

#nullable disable

namespace OLab.Azure.Services;

public class OLabAuthMiddlewareContext : UserContext
{
  // default ctor, needed for services Dependancy Injection
  public OLabAuthMiddlewareContext()
  {

  }

  public OLabAuthMiddlewareContext(
    IOLabLogger logger,
    FunctionContext executionContext,
    OLabDBContext dbContext) : base( logger, dbContext )
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );
    Guard.Argument( executionContext ).NotNull( nameof( executionContext ) );

    GetLogger().LogInformation( $"FunctionUserContext ctor" );

    var executionContextHelper =
      executionContext.Items[ nameof( BootstrapMiddlewareContext ) ] as BootstrapMiddlewareContext;

    LoadHostContext( executionContextHelper );
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

  protected void LoadHostContext(BootstrapMiddlewareContext executionContextHelper)
  {
    var req = executionContextHelper.ExecutionContext.GetHttpRequestData();
    IPAddress = GetRequestIpAddress( req );

    if ( !executionContextHelper.ExecutionContext.Items.TryGetValue( "claims", out var claimsObject ) )
      throw new Exception( "unable to retrieve claims from host context" );

    var claims = claimsObject as IDictionary<string, string>;
    SetClaims( claims );

    var sessionId = executionContextHelper.GetHeader( HEADER_SESSIONID, false );
    if ( sessionId != string.Empty )
    {
      if ( !string.IsNullOrEmpty( sessionId ) && sessionId != "null" )
      {
        SessionId = sessionId;
        if ( !string.IsNullOrWhiteSpace( SessionId ) )
          GetLogger().LogInformation( $"Found {HEADER_SESSIONID} '{SessionId}'." );
      }
    }
    else
      GetLogger().LogWarning( $"no {HEADER_SESSIONID} provided" );

    LoadUserContext();

  }

}

