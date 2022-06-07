using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;


namespace OLabWebAPI.Services
{
  public class OLabUserService : IUserService
  {
    public static int defaultTokenExpiryMinutes = 120;
    private readonly AppSettings _appSettings;
    private readonly OLabDBContext _context;
    private readonly IList<Users> _users;

    public OLabUserService(IOptions<AppSettings> appSettings, OLabDBContext context)
    {
      defaultTokenExpiryMinutes = OLabConfiguration.DefaultTokenExpiryMins;
      _appSettings = appSettings.Value;
      _context = context;

      _users = _context.Users.OrderBy(x => x.Id).ToList();
    }

    /// <summary>
    /// Adds user to database
    /// </summary>
    /// <param name="newUser"></param>
    public void AddUser( Users newUser )
    {

    }

    /// <summary>
    /// Authenticate user
    /// </summary>
    /// <param name="model">Login model</param>
    /// <returns>Authenticate response, or null</returns>
    public AuthenticateResponse Authenticate(LoginRequest model)
    {
      // var user = _users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);
      var user = _users.SingleOrDefault(x => x.Username == model.Username);

      // return null if user not found
      if (user != null)
      {
        if (ValidatePassword(model.Password, user))
        {
          // authentication successful so generate jwt token
          return GenerateJwtToken(user);
        }
      }

      return null;
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

      SHA1 hash = SHA1.Create();
      byte[] plainTextBytes = Encoding.ASCII.GetBytes(clearText);
      byte[] hashBytes = hash.ComputeHash(plainTextBytes);

      user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Get all defined users
    /// </summary>
    /// <returns>Enumerable list of users</returns>
    public IEnumerable<Users> GetAll()
    {
      return _users;
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    /// <param name="id">User id</param>
    /// <returns>User record</returns>
    public Users GetById(int id)
    {
      return _users.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Get user by name
    /// </summary>
    /// <param name="userName">User name</param>
    /// <returns>User record</returns>
    public Users GetByUserName(string userName)
    {
      return _users.FirstOrDefault(x => x.Username == userName);
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
        SHA1 hash = SHA1.Create();
        byte[] plainTextBytes = Encoding.ASCII.GetBytes(clearText);
        byte[] hashBytes = hash.ComputeHash(plainTextBytes);
        string localChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return localChecksum == user.Password;
      }

      return false;
    }

    /// <summary>
    /// Generate JWT token
    /// </summary>
    /// <param name="user">User record from database</param>
    /// <returns>AuthenticateResponse</returns>
    private AuthenticateResponse GenerateJwtToken(Users user)
    {
      // authentication successful so generate jwt token
      var tokenHandler = new JwtSecurityTokenHandler();

      var mySecurityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(_appSettings.Secret[..16]));
      var myIssuer = _appSettings.Issuer;
      var myAudience = _appSettings.Audience;

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.Name, user.Username),
          new Claim(ClaimTypes.Role, $"{user.Group}:{user.Role}")
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = myIssuer,
        Audience = myAudience,
        SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);

      var response = new AuthenticateResponse();
      response.AuthInfo.Token = tokenHandler.WriteToken(token);
      response.AuthInfo.Refresh = null;
      response.Role = $"{user.Group}:{user.Role}";
      response.UserName = user.Username;
      response.AuthInfo.Created = DateTime.UtcNow;
      response.AuthInfo.Expires =
        response.AuthInfo.Created.AddMinutes(defaultTokenExpiryMinutes);

      return response;
    }

    public string GenerateRefreshToken()
    {
      var randomNumber = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }
  }
}