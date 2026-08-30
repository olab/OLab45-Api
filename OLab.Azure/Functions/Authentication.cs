using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public class AuthenticationFunction : OLabFunction
{
  private readonly IOLabAuthentication _authentication;
  private readonly IOLabAuthorization _authorization;

  public AuthenticationFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      IOLabAuthentication authentication,
      OLabDBContext dbContext) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<AuthenticationFunction>( loggerFactory );
    _authentication = authentication;
    _authorization = new OLabAuthorization( Logger, DbContext, configuration );
  }

  [Function( "Login" )]
  public async Task<HttpResponseData> LoginAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/login" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      Logger.LogInformation( $"LoginAsync" );
      var model = await request.ParseBodyFromRequestAsync<LoginRequest>( GetLogger() );

      request.Headers.TryGetValues( "Authorization", out var accessToken );
      var impersonate = accessToken?.Count() > 0;

      if ( impersonate )
      {
        // validate token/setup up common properties
        var auth = GetAuthorization( hostContext );
        if ( !await auth.IsSystemSuperuserAsync() )
        {
          GetLogger().LogWarning( $"User '{auth.OLabUser.Username}' cannot imporsonate" );
          return OLabFunctionResponses.OLabUnauthorizedResponse( request );
        }
      }

      var user = await _authentication.AuthenticateAsync( model, impersonate );
      if ( user == null )
        return OLabFunctionResponses.OLabUnauthorizedResponse( request );

      // test if user has access to application based on referrer URL
      IEnumerable<string> refererValues;
      var referrer = string.Empty;

      if ( request.Headers.TryGetValues( "Referer", out refererValues ) )
      {
        GetLogger().LogInformation( $"referer urls provided: {string.Join( ",", refererValues )}" );
        if ( !await _authorization.HasAccessToAppAsync( user, refererValues.First() ) )
          return OLabFunctionResponses.OLabUnauthorizedResponse( request );
      }
      else
        GetLogger().LogWarning( $"no referer url provided" );

      var authResponse = _authentication.GenerateJwtToken( user, referrer );

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( authResponse ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( LoginAsync ) );
    }

  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginAnonymous" )]
  public async Task<HttpResponseData> LoginAnonymousAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/loginanonymous/{mapId}" )] HttpRequestData request,
    uint mapId,
    CancellationToken cancellationToken)
  {
    GetLogger().LogInformation( $"LoginAnonymous(mapId = '{mapId}')" );

    try
    {
      var response = await _authentication.GenerateAnonymousJwtTokenAsync( mapId );
      if ( response == null )
        return OLabFunctionResponses.OLabUnauthorizedResponse( request );

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( LoginAnonymousAsync ) );
    }

  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginExternal" )]
  public async Task<HttpResponseData> LoginExternalAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/loginexternal" )] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"LoginExternalAsync" );
      var model = await request.ParseBodyFromRequestAsync<ExternalLoginRequest>( GetLogger() );

      var response = _authentication.GenerateExternalJwtToken( model );
      if ( response == null )
        return OLabFunctionResponses.OLabUnauthorizedResponse( request );

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( LoginExternalAsync ) );
    }
  }

}
