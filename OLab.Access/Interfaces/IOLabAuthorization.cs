using Microsoft.AspNetCore.Mvc;
using OLab.Data.Dtos.ScopedObjects;
using OLab.Data.Interface;

namespace OLab.Api.Data.Interface
{
  public interface IOLabAuthorization
  {
    IActionResult HasAccess(string acl, ScopedObjectDto dto);
    bool HasAccess(string acl, string objectType, uint? objectId);
    IUserContext UserContext { get; set; }
    void ApplyUserContext(IUserContext userContext);

  }
}