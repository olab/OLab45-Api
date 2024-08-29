using Dawn;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Services;

public class UserService : IUserService
{
  public static int defaultTokenExpiryMinutes = 120;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger Logger;

  public bool IsValid { get; private set; }
  public bool UserName { get; private set; }
  public bool Role { get; private set; }

  public OLabDBContext GetDbContext() { return _dbContext; }
  public IOLabLogger GetLogger() { return Logger; }

  public UserService(
    ILoggerFactory loggerFactory,
    IOLabConfiguration config,
    OLabDBContext context)
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

  /// <summary>
  /// Authenticate user
  /// </summary>
  /// <param name="model">Login model</param>
  /// <returns>OLab user object, or null, if authenticated</returns>
  public Users Authenticate(LoginRequest model)
  {
    Guard.Argument(model, nameof(model)).NotNull();

    Logger.LogInformation($"Authenticating {model.Username}, ***{model.Password[^3..]}");
    var user = GetDbContext().Users.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower());

    if (user != null)
    {
      if (!ValidatePassword(model.Password, user))
        return null;
    }

    return user;
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
    if (!string.IsNullOrEmpty(user.Salt))
      clearText += user.Salt;

    var hash = SHA1.Create();
    var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
    var hashBytes = hash.ComputeHash(plainTextBytes);

    user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }

  /// <summary>
  /// ReadAsync all defined users
  /// </summary>
  /// <returns>Enumerable list of users</returns>
  public IEnumerable<Users> GetAll()
  {
    return GetDbContext().Users.ToList();
  }

  /// <summary>
  /// ReadAsync user by Id
  /// </summary>
  /// <param name="id">User id</param>
  /// <returns>User record</returns>
  public Users GetById(uint? id)
  {
    if (!id.HasValue)
      return null;
    return GetDbContext()
      .Users
      .Include("UserGrouproles")
      .FirstOrDefault(x => x.Id == id.Value);
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
      .Include("UserGrouproles")
      .FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
  }

  /// <summary>
  /// Validate user password
  /// </summary>
  /// <param name="clearText">Password</param>
  /// <param name="user">Corresponding user record</param>
  /// <returns>true/false</returns>
  public bool ValidatePassword(string clearText, Users user)
  {
    if (!string.IsNullOrEmpty(user.Salt))
    {
      clearText += user.Salt;
      var hash = SHA1.Create();
      var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
      var hashBytes = hash.ComputeHash(plainTextBytes);
      var localChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

      return localChecksum == user.Password;
    }

    return false;
  }

  private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  public static DateTime FromUnixTime(long unixTime)
  {
    return epoch.AddSeconds(unixTime);
  }

  public async Task<List<AddUserResponse>> DeleteUsersAsync(List<DeleteUsersRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug($"DeleteUserAsync(items count '{items.Count}')");

      foreach (var item in items)
      {
        var response = await DeleteUserAsync(item);
        responses.Add(response);
      }

      return responses;
    }
    catch (Exception ex)
    {
      Logger.LogError($"DeleteUserAsync exception {ex.Message}");
      throw;
    }
  }

  public async Task<AddUserResponse> DeleteUserAsync(DeleteUsersRequest userRequest)
  {
    Users user = null;

    // allow for either id or user name to search for
    if ( userRequest.Id > 0 )
      user = GetById(userRequest.Id);
    else if ( !string.IsNullOrEmpty( userRequest.UserName ) )
      user = GetByUserName(userRequest.UserName);

    if (user == null)
    {
      return new AddUserResponse
      {
        Id = userRequest.Id,
        Error = $"User does not exist"
      };
    }

    var physUser =
      await GetDbContext().Users.FirstOrDefaultAsync(x => x.Id == userRequest.Id);

    GetDbContext().Users.Remove(physUser);
    await GetDbContext().SaveChangesAsync();

    var response = new AddUserResponse
    {
      Id = userRequest.Id
    };

    return response;
  }

  public async Task<List<UsersDto>> AddUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<UsersDto>();

      Logger.LogDebug($"AddUserAsync(items count '{items.Count}')");

      foreach (var item in items)
      {
        var user = await AddUserAsync(item);
        responses.Add(user);
      }

      return responses;
    }
    catch (Exception ex)
    {
      Logger.LogError($"AddUserAsync exception {ex.Message}");
      throw;
    }
  }

  /// <summary>
  /// Edit user based on add user request
  /// </summary>
  /// <param name="userRequest">USer request</param>
  /// <returns>Add user response</returns>
  public async Task<UsersDto> EditUserAsync(AddUserRequest userRequest)
  {
    var user = GetById(userRequest.Id);
    if (user == null)
      throw new OLabBadRequestException($"user id: {userRequest.Id} does not exist");


    // need to set the logger and the dbContext since
    // they are not present when AddUserRequest created by webApi
    userRequest.SetInfrastructure(GetLogger(), GetDbContext());

    // parse any GroupRole string(s)
    userRequest.BuildGroupRoleObjects();

    // build physical User object from request
    Users.CreatePhysFromRequest(user, userRequest);

    // update and encrypt password if one was passed in
    if (!string.IsNullOrEmpty(userRequest.Password))
    {
      ChangePassword(user, new ChangePasswordRequest
      {
        NewPassword = userRequest.Password
      });
    }

    GetDbContext().Users.Update(user);
    await GetDbContext().SaveChangesAsync();

    user.UserGrouproles.AddRange(userRequest.GroupRoleObjects);
    GetDbContext().Users.Update(user);
    await GetDbContext().SaveChangesAsync();

    var userDto = new UsersMapper(GetLogger(), GetDbContext()).PhysicalToDto(user);
    return userDto;
  }

  /// <summary>
  /// Add user based on add user request
  /// </summary>
  /// <param name="userRequest">User request</param>
  /// <returns>ADd user response</returns>
  public async Task<UsersDto> AddUserAsync(AddUserRequest userRequest)
  {
    var user = GetByUserName(userRequest.Username);
    if (user != null)
      throw new OLabBadRequestException($"'{userRequest.Username}' already exists");

    var newUserPhys = Users.CreatePhysFromRequest(null, userRequest);
    newUserPhys.UserGrouproles.AddRange(
      UserGrouproles.StringToObjectList(GetDbContext(), userRequest.GroupRoles));

    ChangePassword(newUserPhys, new ChangePasswordRequest
    {
      NewPassword = newUserPhys.Password
    });

    await GetDbContext().Users.AddAsync(newUserPhys);
    await GetDbContext().SaveChangesAsync();

    var userDto = new UsersMapper(GetLogger(), GetDbContext()).PhysicalToDto(newUserPhys);
    return userDto;
  }

  public Task<AddUserResponse> GetUsersAsync(AddUserRequest item)
  {
    throw new NotImplementedException();
  }

  public IList<UsersDto> GetUsers(string name)
  {
    IList<Users> users = new List<Users>();

    if (!string.IsNullOrEmpty(name))
    {
      users = GetDbContext().Users
        .Include("UserGrouproles")
        .Include("UserGrouproles.Group")
        .Include("UserGrouproles.Role")
        .Where(x => x.Nickname.Contains(name) || x.Username.Contains(name)).ToList();
    }
    else
      users = GetDbContext().Users
        .Include("UserGrouproles")
        .Include("UserGrouproles.Group")
        .Include("UserGrouproles.Role")
        .ToList();

    var dtoList = new UsersMapper(GetLogger(), GetDbContext()).PhysicalToDto(users);
    return dtoList;
  }
}