using Dawn;
using DocumentFormat.OpenXml.Drawing;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Dtos;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Management;

public partial class UserManagement : OLabFunction
{
  //protected readonly IUserService _userService;
  private Api.Endpoints.UserEndpoint _userEndpoint;

  public UserManagement(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IOLabAuthorization auth,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<UserManagement>( loggerFactory );

    _userEndpoint = new Api.Endpoints.UserEndpoint(
      Logger,
      configuration,
      auth,
      dbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Query users
  /// </summary>
  /// <param name="request">GetUsersRequest user query</param>
  /// <returns>List of users</returns>
  [Function( "UsersGet" )]
  public async Task<IActionResult> UsersGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "auth/getusers/{name?}" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken,
    string name)
  {
    try
    {
      Logger.LogInformation( $"UsersGet" );

      var auth = GetAuthorization( hostContext );


      // test if user has access to add users.
      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to get user list" ) );

      var dto = await _userEndpoint.GetUsersAsync( name );
      return request
        .CreateResponse( OLabObjectListResult<UsersDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "UsersGet" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Adds users from CSV file
  /// </summary>
  /// <param name="file">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [Function( "ImportUsersPost" )]
  public async Task<IActionResult> ImportUsersPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/importusers" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"ImportUsersPost" );

      var auth = GetAuthorization( hostContext );

      // test if user has access to add users.
      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to import users" ) );

      // Get the Content-Type header
      if ( !request.Headers.TryGetValues( "Content-Type", out var contentTypeValues ) )
        throw new Exception( "Bad Request");

      var contentType = contentTypeValues.First();

      // Parse the form data
      var boundary = GetBoundary( contentType );
      var reader = new MultipartReader( boundary, request.Body );
      MultipartSection section;

      using ( var memoryStream = new MemoryStream() )
      {
        while ( (section = await reader.ReadNextSectionAsync()) != null )
        {
          if ( ContentDispositionHeaderValue.TryParse( section.ContentDisposition, out var contentDisposition ) )
          {
            if ( contentDisposition.DispositionType.Equals( "form-data" ) &&
                contentDisposition.Name == "File" )
            {
              await section.Body.CopyToAsync( memoryStream );
            }
          }
        }

        memoryStream.Position = 0;

        var dto = await _userEndpoint.ImportUsersAsync( memoryStream );
        return request.CreateResponse( OLabObjectListResult<UsersImportDto>.Result( dto ) );
      }

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "ImportUsersPost" );
      return request.CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  private string GetBoundary(string contentType)
  {
    var elements = contentType.Split( ' ' );
    var boundaryElement = elements.FirstOrDefault( entry => entry.StartsWith( "boundary=" ) );
    if ( boundaryElement != null )
    {
      return boundaryElement.Substring( "boundary=".Length );
    }
    throw new InvalidDataException( "Missing content-type boundary" );
  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>AddUserResponse</returns>
  [Function( "UserPost" )]
  public async Task<IActionResult> UserPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/adduser" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"UserPost" );

      var item = await request.ParseBodyFromRequestAsync<AddUserRequest>();
      var auth = GetAuthorization( hostContext );

      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to add user" ) );

      var dto = await _userEndpoint.AddUserAsync( item );
      return request
        .CreateResponse( OLabObjectResult<UsersDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "UserPost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>List of AddUserResponse</returns>
  [Function( "UserDelete" )]
  public async Task<IActionResult> UserDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/deleteuser" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"UserDelete" );

      var items = await request.ParseBodyFromRequestAsync<List<DeleteUsersRequest>>();
      var auth = GetAuthorization( hostContext );

      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to add user" ) );

      var responses = await _userEndpoint.DeleteUsersAsync( items );
      return request
        .CreateResponse( OLabObjectListResult<AddUserResponse>.Result( responses ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "UserDelete" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <returns>AddUserResponse</returns>
  [Function( "UsersPost" )]
  public async Task<IActionResult> UsersPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/addusers" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"UsersPost" );

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

          var dto = await _userEndpoint.AddUsersAsync( items );
          return request
            .CreateResponse( OLabObjectListResult<UsersDto>.Result( dto ) );

        }
      }

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "UsersPost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <param name="jsonStringData">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [Function( "UsersPut" )]
  public async Task<IActionResult> UsersPutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "auth/edituser" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"UsersPut" );

      var body = await request.ParseBodyFromRequestAsync<AddUserRequest>();
      var auth = GetAuthorization( hostContext );

      // test if user has access to add users.
      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to edit users" ) );

      var dto = await _userEndpoint.EditUserAsync( body );
      return request
        .CreateResponse( OLabObjectResult<UsersDto>.Result( dto ) );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "UsersPut" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
