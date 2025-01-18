using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Data.Interface;
using OLab.Access;
using OLab.Azure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Humanizer;
using OLab.Api.Dto;

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

      Logger.LogInformation( $"Login(user = '{model.Username}' ip: ???)" );

      var user = _authentication.Authenticate( model );
      if ( user == null )
        return OLabUnauthorizedResult.Result();

      // test if user has access to application based on referrer URL
      IEnumerable<string> refererValues;
      var referrer = string.Empty;

      if ( request.Headers.TryGetValues( "Referer", out refererValues ) )
      {
        referrer = _authorization.ExtractApplication( refererValues.First() );
        if ( !await _authorization.HasAccessToAppAsync( user, referrer ) )
          return OLabUnauthorizedResult.Result();
      }
      else
        Logger.LogInformation( $"no referer url provided" );

      var response = _authentication.GenerateJwtToken( user, referrer );

      var json = JsonConvert.SerializeObject( response );
      return new OkObjectResult( json );
    }
    catch ( Exception ex )
    {
      return OLabServerErrorResult.Result( ex.Message );
    }
  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginAnonymous" )]
  public IActionResult LoginAnonymous(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/loginanonymous/{mapId}" )] HttpRequestData request,
    uint mapId,
    CancellationToken cancellationToken)
  {
    Logger.LogInformation( $"LoginAnonymous(mapId = '{mapId}')" );

    try
    {
      var response = _authentication.GenerateAnonymousJwtToken( mapId );
      if ( response == null )
        return OLabUnauthorizedResult.Result();

      return OLabObjectResult<AuthenticateResponse>.Result( response );
    }
    catch ( Exception ex )
    {
      return OLabServerErrorResult.Result( ex.Message );
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
      Logger.LogInformation( $"LoginExternal(user = '{model.ExternalToken}')" );

      var response = _authentication.GenerateExternalJwtToken( model );
      if ( response == null )
        return OLabUnauthorizedResult.Result();

      return OLabObjectResult<AuthenticateResponse>.Result( response );
    }
    catch ( Exception ex )
    {
      return OLabServerErrorResult.Result( ex.Message );
    }
  }

}
