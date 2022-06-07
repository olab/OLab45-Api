using System;
using System.Linq;

namespace OLabWebAPI.Model
{
  public partial class Users
  {
    public const int SaltLength = 64;
    public const int PasswordLength = 8;
    private static readonly Random random = new Random();

    public static string RandomString(int length)
    {
      const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
      return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static Users CreateDefault(AddUserRequest model)
    {
      var newUser = CreateDefault();

      if (!string.IsNullOrEmpty(model.NewPassword))
        newUser.Password = model.NewPassword;

      newUser.Username = model.Username;
      newUser.Nickname = model.NickName;
      newUser.Email = model.EMail;
      newUser.Group = model.Group;
      newUser.Role = model.Role;

      return newUser;
    }

    public static Users CreateDefault()
    {
      var user = new Users
      {
        Salt = RandomString(SaltLength),
        Password = RandomString(PasswordLength),
        ModeUi = "easy"
      };

      return user;
    }
  }
}