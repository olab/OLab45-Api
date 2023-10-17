using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OLab.Api.Dto;

namespace OLab.Api.Data.Interface
{
  public interface IOLabAuthorization
  {
    IActionResult HasAccess(string acl, ScopedObjectDto dto);
    bool HasAccess(string acl, string objectType, uint? objectId);
    IUserContext GetUserContext();
  }
}