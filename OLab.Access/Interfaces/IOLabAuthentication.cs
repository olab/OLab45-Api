using Microsoft.IdentityModel.Tokens;
using OLab.Api.Model;
using System.Collections.Generic;
using System.Security.Claims;

namespace OLab.Access.Interfaces;

public interface IOLabAuthentication
{
  string ExtractAccessToken(
    IReadOnlyDictionary<string, string> headers,
    IReadOnlyDictionary<string, object> bindingData);
  bool ValidateToken(string token);
  IDictionary<string, string> Claims { get; }
  TokenValidationParameters GetValidationParameters();
  AuthenticateResponse GenerateJwtToken(Users user, string issuedBy = "olab");

}