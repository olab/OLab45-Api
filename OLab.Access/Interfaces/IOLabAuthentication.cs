using Microsoft.IdentityModel.Tokens;
using OLab.Api.Model;
using System.Collections.Generic;

namespace OLab.Access.Interfaces;

public interface IOLabAuthentication
{
  Users Authenticate(LoginRequest model);

  string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers,
    IReadOnlyDictionary<string, object> bindingData = null);
  bool ValidateToken(string token);
  IDictionary<string, string> Claims { get; }
  TokenValidationParameters GetValidationParameters();
  AuthenticateResponse GenerateJwtToken(Users user, string issuedBy = "olab");
  AuthenticateResponse GenerateAnonymousJwtToken(uint mapId);
  AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model);

}