using Microsoft.IdentityModel.Tokens;
using OLab.Api.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OLab.Access.Interfaces;

public interface IOLabAuthentication
{
  Task<Users> AuthenticateAsync(LoginRequest model, bool impersonateMode = false);

  string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers,
    IReadOnlyDictionary<string, object> bindingData = null);
  bool ValidateToken(string token);
  bool UpdatePassword(string newPassword, Users physUser);

  IDictionary<string, string> Claims { get; }
  TokenValidationParameters GetValidationParameters();
  AuthenticateResponse GenerateJwtToken(Users user, string referrer, string issuedBy = "olab");
  Task<AuthenticateResponse> GenerateAnonymousJwtTokenAsync(uint mapId);
  AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model);

}