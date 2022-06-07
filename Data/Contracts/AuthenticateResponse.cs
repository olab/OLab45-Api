using System;

namespace OLabWebAPI.Model
{
  public class AuthenticateResponse
  {
    public DateTime CreatedAt { get; set;}
    public string UserName { get; set; }
    public string Role { get; set; }
    public RefreshToken AuthInfo { get; set; }

    public AuthenticateResponse()
    {
      CreatedAt = DateTime.Now;
      AuthInfo = new RefreshToken();
    }

  }
}