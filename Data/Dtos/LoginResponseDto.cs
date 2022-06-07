using OLabWebAPI.Model;

namespace OLabWebAPI.Dto
{
  public class LoginResponseDto
  {
    public LoginResponseDto()
    {
      AuthInfo = new RefreshToken();
    }

    public string UserName { get; set; }
    public string Group { get; set; }
    public string Role { get; set; }
    public RefreshToken AuthInfo { get; set; }
  }
}
