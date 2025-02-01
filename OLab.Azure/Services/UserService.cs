using Dawn;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Dtos;
using OLab.Data.Interface;
using OLab.Data.Mappers;
using System.Security.Cryptography;
using System.Text;
using Users = OLab.Api.Model.Users;

namespace OLab.Azure.Services;

public class UserService : IUserService
{
  public static int defaultTokenExpiryMinutes = 120;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger Logger;

  public OLabDBContext GetDbContext() { return _dbContext; }
  public IOLabLogger GetLogger() { return Logger; }

  public bool IsValid { get; private set; }
  public bool UserName { get; private set; }
  public bool Role { get; private set; }

  public UserService(
    ILoggerFactory loggerFactory,
    OLabDBContext context,
    IOLabConfiguration config)
  {
    try
    {
      Logger = OLabLogger.CreateNew<UserService>( loggerFactory );
      Logger.LogInformation( $"UserService ctor" );

      Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
      Guard.Argument( context ).NotNull( nameof( context ) );
      Guard.Argument( config ).NotNull( nameof( config ) );

      _dbContext = context;
      _config = config;

      defaultTokenExpiryMinutes = _config.GetAppSettings().TokenExpiryMinutes;

      Logger.LogInformation( $"appSetting aud: '{_config.GetAppSettings().Audience}', secret: '{_config.GetAppSettings().Secret[ ..4 ]}...'" );

    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, $"{this.GetType().Name} exception" );
      throw;
    }

  }

  /// <summary>
  /// Adds a list of users asynchronously.
  /// </summary>
  /// <param name="items">List of user requests to add.</param>
  /// <returns>A list of user DTOs representing the added users.</returns>
  /// <exception cref="Exception">Thrown when an error occurs while adding users.</exception>
  public async Task<List<UsersDto>> AddUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<UsersDto>();

      Logger.LogInformation( $"AddUserAsync(items count '{items.Count}')" );

      foreach ( var item in items )
      {
        var user = await AddUserAsync( item );
        responses.Add( user );
      }

      return responses;
    }
    catch ( Exception ex )
    {
      Logger.LogError( $"AddUserAsync exception {ex.Message}" );
      throw;
    }
  }

  /// <summary>
  /// ReadAsync user by Id
  /// </summary>
  /// <param name="id">User id</param>
  /// <returns>User record</returns>
  public Users GetById(uint? id)
  {
    if ( !id.HasValue )
      return null;
    return GetDbContext()
      .Users
      .Include( "UserGrouproles" )
      .FirstOrDefault( x => x.Id == id.Value );
  }

  /// <summary>
  /// Retrieves a list of users based on the provided name.
  /// </summary>
  /// <param name="name">The name to search for. If null or empty, all users are returned.</param>
  /// <returns>A list of user DTOs that match the search criteria.</returns>
  public IList<UsersDto> GetUsers(string name)
  {
    IList<Users> users = new List<Users>();

    if ( !string.IsNullOrEmpty( name ) )
    {
      users = GetDbContext().Users
        .Include( "UserGrouproles" )
        .Include( "UserGrouproles.Group" )
        .Include( "UserGrouproles.Role" )
        .Where( x => x.Nickname.Contains( name ) || x.Username.Contains( name ) ).ToList();
    }
    else
      users = GetDbContext().Users
        .Include( "UserGrouproles" )
        .Include( "UserGrouproles.Group" )
        .Include( "UserGrouproles.Role" )
        .ToList();

    var dtoList = new UsersMapper( GetLogger(), GetDbContext() ).PhysicalToDto( users );
    return dtoList;
  }

  /// <summary>
  /// Updates a user record with a new password
  /// </summary>
  /// <param name="user">Existing user record from DB</param>
  /// <param name="model">Change password request model</param>
  /// <returns></returns>
  public void ChangePassword(Users user, ChangePasswordRequest model)
  {
    var clearText = model.NewPassword;

    // add password salt, if it's defined
    if ( !string.IsNullOrEmpty( user.Salt ) )
      clearText += user.Salt;

    var hash = SHA1.Create();
    var plainTextBytes = Encoding.ASCII.GetBytes( clearText );
    var hashBytes = hash.ComputeHash( plainTextBytes );

    user.Password = BitConverter.ToString( hashBytes ).Replace( "-", "" ).ToLowerInvariant();
  }

  /// <summary>
  /// Add user based on add user request
  /// </summary>
  /// <param name="userRequest">User request</param>
  /// <returns>ADd user response</returns>
  public async Task<UsersDto> AddUserAsync(AddUserRequest userRequest)
  {
    var user = GetByUserName( userRequest.Username );
    if ( user != null )
      throw new OLabBadRequestException( $"'{userRequest.Username}' already exists" );

    Logger.LogInformation( $"adding user '{userRequest.Username}'" );

    var newUserPhys = Users.CreatePhysFromRequest( null, userRequest );
    newUserPhys.UserGrouproles.AddRange(
      UserGrouproles.StringToObjectList( GetDbContext(), userRequest.GroupRoles ) );

    // if salt not passed in, then the incoming password is 
    // cleartext, so we need to do a 'change password'
    // on it to convert it to a hash before saving to database.
    if ( string.IsNullOrEmpty( newUserPhys.Salt ) )
    {
      ChangePassword( newUserPhys, new ChangePasswordRequest
      {
        NewPassword = newUserPhys.Password
      } );
    }

    await GetDbContext().Users.AddAsync( newUserPhys );
    await GetDbContext().SaveChangesAsync();

    var userDto = new UsersMapper( GetLogger(), GetDbContext() ).PhysicalToDto( newUserPhys );
    return userDto;
  }

  /// <summary>
  /// Deletes a user asynchronously based on the provided user request.
  /// </summary>
  /// <param name="userRequest">The request containing the user information to delete.</param>
  /// <returns>The task result contains the response of the delete operation.</returns>
  /// <exception cref="Exception">Thrown when an error occurs while deleting the user.</exception>
  public async Task<AddUserResponse> DeleteUserAsync(DeleteUsersRequest userRequest)
  {
    Logger.LogInformation( $" deleting user '{userRequest.UserName}'" );

    Users user = null;

    // allow for either id or user name to search for
    if ( userRequest.Id > 0 )
      user = GetById( userRequest.Id );
    else if ( !string.IsNullOrEmpty( userRequest.UserName ) )
      user = GetByUserName( userRequest.UserName );

    if ( user == null )
    {
      return new AddUserResponse
      {
        Id = userRequest.Id,
        Error = $"User does not exist"
      };
    }

    var physUser =
      await GetDbContext().Users.FirstOrDefaultAsync( x => x.Id == userRequest.Id );

    GetDbContext().Users.Remove( physUser );
    await GetDbContext().SaveChangesAsync();

    var response = new AddUserResponse
    {
      Id = userRequest.Id
    };

    return response;
  }

  /// <summary>
  /// Deletes a list of users asynchronously based on the provided user requests.
  /// </summary>
  /// <param name="items">The list of user requests containing the user information to delete.</param>
  /// <returns>A task that represents the asynchronous operation. The task result contains a list of responses for each delete operation.</returns>
  /// <exception cref="Exception">Thrown when an error occurs while deleting the users.</exception>
  public async Task<List<AddUserResponse>> DeleteUsersAsync(List<DeleteUsersRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogInformation( $"DeleteUserAsync(items count '{items.Count}')" );

      foreach ( var item in items )
      {
        var response = await DeleteUserAsync( item );
        responses.Add( response );
      }

      return responses;
    }
    catch ( Exception ex )
    {
      Logger.LogError( $"DeleteUserAsync exception {ex.Message}" );
      throw;
    }
  }

  /// <summary>
  /// Imports users from an Excel file asynchronously.
  /// </summary>
  /// <param name="fileStream">The stream of the Excel file containing user data.</param>
  /// <returns>A task that represents the asynchronous operation. The task result contains a list of user import DTOs representing the imported users.</returns>
  /// <exception cref="Exception">Thrown when an error occurs while importing users.</exception>
  public async Task<List<UsersImportDto>> ImportUsersAsync(Stream fileStream)
  {
    var responses = new List<UsersImportDto>();

    using ( var spreadsheetDocument = SpreadsheetDocument.Open( fileStream, false ) )
    {

      var workbookPart =
        spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
      var worksheetPart = workbookPart.WorksheetParts.First();
      var sheet = worksheetPart.Worksheet;

      var sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
      var sst = sstpart.SharedStringTable;

      var cells = sheet.Descendants<Cell>();
      var rows = sheet.Descendants<Row>();

      Logger.LogInformation( $"Import row count = {rows.LongCount()}" );
      Logger.LogInformation( $"       cell count = {cells.LongCount()}" );

      foreach ( var row in rows )
      {
        var column = 0;
        var userRequest = new AddUserRequest(
          Logger,
          GetDbContext() );

        var groupRoleStrings = new List<string>();

        foreach ( var c in row.Elements<Cell>() )
        {
          if ( c.DataType != null && c.DataType == CellValues.SharedString )
          {
            var ssid = int.Parse( c.CellValue.Text );
            var str = sst.ChildElements[ ssid ].InnerText;

            switch ( column )
            {
              case 0:
                userRequest.Operation = str;
                break;
              case 1:
                userRequest.Username = str;
                break;
              case 2:
                userRequest.NickName = str;
                break;
              case 3:
                userRequest.EMail = str;
                break;
              case 4:
                userRequest.Password = str;
                break;
              default:
                groupRoleStrings.Add( str );
                break;
            }

          }

          column++;
        }

        userRequest.GroupRoles = string.Join( ",", groupRoleStrings );

        if ( string.IsNullOrEmpty( userRequest.Operation ) || userRequest.Operation == "+" )
          try
          {
            var response = await AddUserAsync( userRequest );
            responses.Add( new UsersImportDto( response ) { Message = "added" } );
          }
          catch ( Exception ex )
          {
            responses.Add( new UsersImportDto
            {
              UserName = userRequest.Username,
              Status = false,
              Message = ex.Message
            } );
          }

        else if ( userRequest.Operation == "*" )
          try
          {
            var response = await EditUserAsync( userRequest );

            // test if user previously added (in the responses), if so then
            // remove previous before adding edited user
            var existingUser = responses.FirstOrDefault( x => x.Id == response.Id );
            if ( existingUser != null )
            {
              responses.Remove( existingUser );
              responses.Add( new UsersImportDto( response ) { Message = "added, edited" } );
            }
            else
              responses.Add( new UsersImportDto( response ) { Message = "edited" } );

          }
          catch ( Exception ex )
          {
            responses.Add( new UsersImportDto
            {
              UserName = userRequest.Username,
              Status = false,
              Message = ex.Message
            } );
          }

        else if ( userRequest.Operation == "-" )
          try
          {
            var list = new List<DeleteUsersRequest>();
            list.Add( new DeleteUsersRequest { UserName = userRequest.Username } );
            await DeleteUsersAsync( list );

            //responses.Add(new UsersImportDto
            //{
            //  UserName = userRequest.Username,
            //  Message = "deleted"
            //});

          }
          catch ( Exception ex )
          {
            responses.Add( new UsersImportDto
            {
              UserName = userRequest.Username,
              Status = false,
              Message = ex.Message
            } );
          }
      }
    }

    return responses;
  }

  /// <summary>
  /// ReadAsync user by name
  /// </summary>
  /// <param name="userName">User name</param>
  /// <returns>User record</returns>
  public Users GetByUserName(string userName)
  {
    return GetDbContext()
      .Users
      .Include( "UserGrouproles" )
      .FirstOrDefault( x => x.Username.ToLower() == userName.ToLower() );
  }

  /// <summary>
  /// Edit user based on add user request
  /// </summary>
  /// <param name="userRequest">USer request</param>
  /// <returns>Add user response</returns>
  public async Task<UsersDto> EditUserAsync(AddUserRequest userRequest)
  {
    var user = GetByUserName( userRequest.Username );
    if ( user == null )
      throw new OLabBadRequestException( $"user: '{userRequest.Username}' does not exist" );

    Logger.LogInformation( $"editing user '{userRequest.Username}'" );

    // need to set the logger and the dbContext since
    // they are not present when AddUserRequest created by webApi
    userRequest.SetInfrastructure( GetLogger(), GetDbContext() );

    // parse any GroupRole string(s)
    userRequest.BuildGroupRoleObjects();

    // build physical User object from request
    Users.CreatePhysFromRequest( user, userRequest );

    // update and encrypt password if one was passed in
    if ( !string.IsNullOrEmpty( userRequest.Password ) )
    {
      ChangePassword( user, new ChangePasswordRequest
      {
        NewPassword = userRequest.Password
      } );
    }

    GetDbContext().Users.Update( user );
    await GetDbContext().SaveChangesAsync();

    user.UserGrouproles.AddRange( userRequest.GroupRoleObjects );
    GetDbContext().Users.Update( user );
    await GetDbContext().SaveChangesAsync();

    var userDto = new UsersMapper( GetLogger(), GetDbContext() ).PhysicalToDto( user );
    return userDto;
  }


}