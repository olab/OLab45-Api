using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using HttpMultipartParser;
using System.IO;
using System.Text;
using Microsoft.Extensions.Primitives;
using Azure.Core;
using System.Net;
using OLab.Api.Data.Interface;
using OLab.Access;
using OLab.Data.Dtos;

namespace OLab.FunctionApp.Functions.API;

public class UserFunction : OLabFunction
{
  protected readonly IUserService _userService;
  private readonly IOLabAuthentication _authentication;
  private readonly IOLabAuthorization _authorization;

  public UserFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      IUserService userService,
      IOLabAuthentication authentication,
      OLabDBContext dbContext) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<UserFunction>( loggerFactory );
    _authentication = authentication;
    _userService = userService;

    _authorization = new OLabAuthorization( Logger, DbContext, configuration );
  }

  [Function( "Login" )]
  public async Task<HttpResponseData> LoginAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/login" )] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );

      var model = await request.ParseBodyFromRequestAsync<LoginRequest>();

      Logger.LogDebug( $"Login(user = '{model.Username}' ip: ???)" );

      var user = _authentication.Authenticate( model );
      if ( user == null )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Username or password is incorrect" ) );

      // test if user has access to application based on referrer URL
      IEnumerable<string> refererValues;
      string referrer = string.Empty;

      if ( request.Headers.TryGetValues( "Referer", out refererValues ) )
      {
        referrer = _authorization.ExtractApplication( refererValues.First() );
        if ( !await _authorization.HasAccessToAppAsync( user, referrer ) )
          return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "User does not have access to this application" ) );
      }
      else
        Logger.LogInformation( $"no referer url provided" );

      var response = _authentication.GenerateJwtToken( user, referrer );
      return request.CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );

    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;

  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginAnonymous" )]
  public HttpResponseData LoginAnonymous(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/loginanonymous/{mapId}" )] HttpRequestData request,
    uint mapId,
    CancellationToken cancellationToken)
  {
    Logger.LogDebug( $"LoginAnonymous(mapId = '{mapId}')" );

    try
    {
      var response = _authentication.GenerateAnonymousJwtToken( mapId );
      if ( response == null )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Must be Logged on to Play Map" ) );

      return request.CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );

    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;
  }

  /// <summary>
  /// Anonymous login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [Function( "LoginExternal" )]
  public async Task<HttpResponseData> LoginExternalAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/loginexternal" )] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      var model = await request.ParseBodyFromRequestAsync<ExternalLoginRequest>();
      Logger.LogDebug( $"LoginExternal(user = '{model.ExternalToken}')" );

      var response = _authentication.GenerateExternalJwtToken( model );
      if ( response == null )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Invalid external token" ) );

      return request.CreateResponse( OLabObjectResult<AuthenticateResponse>.Result( response ) );

    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;
  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>AddUserResponse</returns>
  [Function( "AddUser" )]
  public async Task<HttpResponseData> AddUserAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/adduser" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogDebug( $"AddUserAsync" );

      var item = await request.ParseBodyFromRequestAsync<AddUserRequest>();
      var auth = GetAuthorization( hostContext );

      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to add user" ) );

      var responses = await _userService.AddUserAsync( item );
      response = request.CreateResponse( OLabObjectResult<UsersDto>.Result( responses ) );
    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;

  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>List of AddUserResponse</returns>
  [Function( "DeleteUser" )]
  public async Task<HttpResponseData> DeleteUserAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/deleteuser" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogDebug( $"DeleteUserAsync" );

      var items = await request.ParseBodyFromRequestAsync<List<DeleteUsersRequest>>();
      var auth = GetAuthorization( hostContext );

      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to add user" ) );

      var responses = await _userService.DeleteUsersAsync( items );
      response = request.CreateResponse( OLabObjectListResult<AddUserResponse>.Result( responses ) );
    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;

  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>AddUserResponse</returns>
  [Function( "AddUsers" )]
  public async Task<HttpResponseData> AddUsersAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/addusers" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogDebug( $"AddUsersAsync" );

      var responses = new List<AddUserResponse>();
      var auth = GetAuthorization( hostContext );

      // test if user has access to add users.
      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to add users" ) );

      var parser = await MultipartFormDataParser.ParseAsync( request.Body );
      if ( parser.Files.Count == 0 )
        throw new Exception( "No files were uploaded" );

      using ( var stream = parser.Files[ 0 ].Data )
      {
        using ( var streamReader = new StreamReader( stream, Encoding.UTF8 ) )
        {
          var items = new List<AddUserRequest>();

          String userRequestText;
          while ( (userRequestText = streamReader.ReadLine()) != null )
          {
            var userRequest = new AddUserRequest(
              Logger,
              DbContext );

            items.Add( userRequest );
          }

          var result = await _userService.AddUsersAsync( items );
          response = request.CreateResponse( OLabObjectListResult<UsersDto>.Result( result ) );

        }
      }

    }
    catch ( Exception ex )
    {
      response = request.CreateResponse( ex );
    }

    return response;

  }
}
