using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public class AuthenticationFunction : OLabFunction
{
  protected readonly IUserService _userService;
  private readonly IOLabAuthentication _authentication;
  private readonly IOLabAuthorization _authorization;

  public AuthenticationFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      IUserService userService,
      IOLabAuthentication authentication,
      OLabDBContext dbContext) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<AuthenticationFunction>( loggerFactory );
    _authentication = authentication;
    _userService = userService;

    _authorization = new OLabAuthorization( Logger, DbContext, configuration );
  }

  [Function( "Login" )]
  public async Task<IActionResult> LoginAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/login" )] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      var model = await request.ParseBodyFromRequestAsync<LoginRequest>();

      GetLogger().LogInformation( $"Login(user = '{model.Username}' ip: ???)" );

      var user = await _authentication.AuthenticateAsync( model );
      if ( user == null )
        return OLabUnauthorizedResult.Result();

      // test if user has access to application based on referrer URL
      IEnumerable<string> refererValues;
      var referrer = string.Empty;

      if ( request.Headers.TryGetValues( "Referer", out refererValues ) )
      {
        GetLogger().LogInformation( $"referer urls provided: {string.Join( ",", refererValues )}" );
        if ( !await _authorization.HasAccessToAppAsync( user, refererValues.First() ) )
          return OLabUnauthorizedResult.Result();
      }
      else
        GetLogger().LogInformation( $"no referer url provided" );

      var authResponse = _authentication.GenerateJwtToken( user, referrer );

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( authResponse ) );
    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex, "Login" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginAnonymous" )]
  public async Task<IActionResult> LoginAnonymousAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/loginanonymous/{mapId}" )] HttpRequestData request,
    uint mapId,
    CancellationToken cancellationToken)
  {
    GetLogger().LogInformation( $"LoginAnonymous(mapId = '{mapId}')" );

    try
    {
      var response = await _authentication.GenerateAnonymousJwtTokenAsync( mapId );
      if ( response == null )
        return OLabUnauthorizedResult.Result();

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );
    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex, "LoginAnonymous" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginExternal" )]
  public async Task<IActionResult> LoginExternalAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/loginexternal" )] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      var model = await request.ParseBodyFromRequestAsync<ExternalLoginRequest>();
      GetLogger().LogInformation( $"LoginExternal(user = '{model.ExternalToken}')" );

      var response = _authentication.GenerateExternalJwtToken( model );
      if ( response == null )
        return OLabUnauthorizedResult.Result();

      return request
        .CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );
    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex, "LoginExternalAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

}
