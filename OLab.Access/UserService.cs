using Dawn;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Access;

public class UserService : IUserService
{
  public static int defaultTokenExpiryMinutes = 120;
  protected readonly OLabDBContext _dbContext;
  protected readonly IOLabConfiguration _config;
  protected readonly IOLabLogger Logger;

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
    return _dbContext.Users
      .Include("UserGroups")
      .Include("UserGroups.Group")
      .ToList();
  }

  /// <summary>
  /// Get user by Id
  /// </summary>
  /// <param name="id">User id</param>
  /// <returns>User record</returns>
  public Users GetById(int id)
  {
    return _dbContext.Users
      .Include("UserGroups")
      .Include("UserGroups.Group")
      .FirstOrDefault(x => x.Id == id);
  }

  /// <summary>
  /// Get user by name
  /// </summary>
  /// <param name="userName">User name</param>
  /// <returns>User record</returns>
  public Users GetByUserName(string userName)
  {
    return _dbContext.Users
      .Include("UserGroups")
      .Include("UserGroups.Group")
      .FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
  }

  public async Task<List<AddUserResponse>> DeleteUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug($"DeleteUserAsync(items count '{items.Count}')");

      foreach (AddUserRequest item in items)
      {
        AddUserResponse response = await DeleteUserAsync(item);
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

  public async Task<AddUserResponse> DeleteUserAsync(AddUserRequest userRequest)
  {
    Users user = GetByUserName(userRequest.Username);
    if (user == null)
    {
      return new AddUserResponse
      {
        Username = userRequest.Username.ToLower(),
        Message = $"User does not exist"
      };
    }

    var physUser =
      await _dbContext.Users
        .Include("UserGroups")
        .Include("UserGroups.Group")
        .FirstOrDefaultAsync(x => x.Username == userRequest.Username);

    _dbContext.Users.Remove(physUser);
    await _dbContext.SaveChangesAsync();

    var response = new AddUserResponse
    {
      Username = physUser.Username,
      Message = "Deleted"
    };

    return response;
  }

  public async Task<List<AddUserResponse>> AddUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug($"AddUserAsync(items count '{items.Count}')");

      foreach (AddUserRequest item in items)
      {
        AddUserResponse response = await AddUserAsync(item);
        responses.Add(response);
      }

      return responses;
    }
    catch (Exception ex)
    {
      Logger.LogError($"AddUserAsync exception {ex.Message}");
      throw;
    }
  }

  public async Task<AddUserResponse> AddUserAsync(AddUserRequest userRequest)
  {
    Users user = GetByUserName(userRequest.Username);
    if (user != null)
    {
      return new AddUserResponse
      {
        Username = userRequest.Username.ToLower(),
        Message = $"Already exists"
      };
    }

    var newUser = Users.CreateDefault(userRequest);
    var newPassword = newUser.Password;

    ChangePassword(newUser, new ChangePasswordRequest
    {
      NewPassword = newUser.Password
    });

    await _dbContext.Users.AddAsync(newUser);
    await _dbContext.SaveChangesAsync();

    AttachUserRoles(newUser, userRequest);
    await _dbContext.SaveChangesAsync();

    var response = new AddUserResponse
    {
      Id = newUser.Id,
      Username = newUser.Username,
      Password = newPassword,
      Roles = UserGroups.GetGroupRole(newUser.UserGroups.ToList())
    };

    return response;
  }

  private Groups GetBuildGroup(string groupName)
  {
    var groupPhys = _dbContext.Groups.FirstOrDefault(x => x.Name == groupName);
    if (groupPhys == null)
    {
      _dbContext.Groups.Add(new Groups { Name = groupName });
      _dbContext.SaveChanges();

      groupPhys = _dbContext.Groups.FirstOrDefault(x => x.Name == groupName);
    }

    return groupPhys;
  }

  private SecurityRoles BuildRoleAcls(Groups groupPhys, string role)
  {
    var securityRolePhys = _dbContext.SecurityRoles.FirstOrDefault(x =>
      x.GroupId == groupPhys.Id &&
      x.ImageableId == 0 &&
      x.ImageableType == Constants.ScopeLevelMap);

    // create default map ACL for new group/role
    // only if superuser or author
    if ((role == SecurityRoles.RoleSuperuser) ||
         (role == SecurityRoles.RoleAuthor))
    {
      if (securityRolePhys == null)
      {
        securityRolePhys = new SecurityRoles
        {
          GroupId = groupPhys.Id,
          ImageableId = 0,
          ImageableType = Constants.ScopeLevelMap,
          Acl = "RWXD",
          Role = role
        };

        // create default security role for group
        _dbContext.SecurityRoles.Add(securityRolePhys);
        _dbContext.SaveChanges();

      }

    }

    return securityRolePhys;
  }


  private void AttachUserRoles(Users user, AddUserRequest userRequest)
  {
    if (userRequest.Roles.Count == 0)
    {
      if (string.IsNullOrEmpty(userRequest.Group))
        throw new Exception("Missing user group");

      if (string.IsNullOrEmpty(userRequest.Role))
        throw new Exception("Missing user role");

      var groupPhys = GetBuildGroup(userRequest.Group);
      var securityRoleAcl = BuildRoleAcls(groupPhys, userRequest.Role);

      user.UserGroups.Add(new UserGroups
      {
        GroupId = groupPhys.Id,
        Role = userRequest.Role,
      });

      Logger.LogInformation($"  Added group/role {groupPhys.Id}/{userRequest.Role} to user {user.Username}");

    }

    foreach (var role in userRequest.Roles)
    {
      var groupPhys = _dbContext.Groups.FirstOrDefault(x => x.Name == role.Group);
      if (groupPhys == null)
        groupPhys = _dbContext.Groups.FirstOrDefault();

      user.UserGroups.Add(new UserGroups
      {
        GroupId = groupPhys.Id,
        Role = role.Role,
      });

      Logger.LogInformation($"  Added group/role {groupPhys.Id}/{role.Role} to user {user.Username}");

    }
  }
}