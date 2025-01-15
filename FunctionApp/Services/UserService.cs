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

namespace OLab.FunctionApp.Services;

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
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(context).NotNull(nameof(context));
    Guard.Argument(config).NotNull(nameof(config));

    _dbContext = context;
    _config = config;

    defaultTokenExpiryMinutes = config.GetAppSettings().TokenExpiryMinutes;

    Logger = OLabLogger.CreateNew<UserService>(loggerFactory);

    Logger.LogInformation($"UserService ctor");
    Logger.LogInformation($"appSetting aud: '{config.GetAppSettings().Audience}', secret: '{config.GetAppSettings().Secret[..4]}...'");

  }

  public async Task<List<UsersImportDto>> ImportUsersAsync(Stream fileStream)
  {
    var responses = new List<UsersImportDto>();

    using ( SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open( fileStream, false ) )
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

      foreach ( Row row in rows )
      {
        int column = 0;
        var userRequest = new AddUserRequest(
          Logger,
          GetDbContext() );

        var groupRoleStrings = new List<string>();

        foreach ( Cell c in row.Elements<Cell>() )
        {
          if ( (c.DataType != null) && (c.DataType == CellValues.SharedString) )
          {
            int ssid = int.Parse( c.CellValue.Text );
            string str = sst.ChildElements[ ssid ].InnerText;

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
        {
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
        }

        else if ( userRequest.Operation == "*" )
        {
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
        }

        else if ( userRequest.Operation == "-" )
        {
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
    }

    return responses;
  }

  /// <summary>
  /// Updates a user record with a new password
  /// </summary>
  /// <param name="user">Existing user record from DB</param>
  /// <param name="model">Change password request model</param>
  /// <returns></returns>
  public void ChangePassword(Users user, ChangePasswordRequest model)
  {
    Guard.Argument(user, nameof(user)).NotNull();
    Guard.Argument(model, nameof(model)).NotNull();

    var clearText = model.NewPassword;

    // add password salt, if it's defined
    if (!string.IsNullOrEmpty(user.Salt))
      clearText += user.Salt;

    var hash = SHA1.Create();
    var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
    var hashBytes = hash.ComputeHash(plainTextBytes);

    user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }

  /// <summary>
  /// Get all defined users
  /// </summary>
  /// <returns>Enumerable list of users</returns>
  public IEnumerable<Users> GetAll()
  {
    return _dbContext.Users.ToList();
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
  /// Get user by name
  /// </summary>
  /// <param name="userName">User name</param>
  /// <returns>User record</returns>
  public Users GetByUserName(string userName)
  {
    return _dbContext.Users.FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
  }

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

  public async Task<List<AddUserResponse>> DeleteUsersAsync(List<DeleteUsersRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug( $"DeleteUserAsync(items count '{items.Count}')" );

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

  public async Task<List<UsersDto>> AddUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<UsersDto>();

      Logger.LogDebug( $"AddUserAsync(items count '{items.Count}')" );

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

}