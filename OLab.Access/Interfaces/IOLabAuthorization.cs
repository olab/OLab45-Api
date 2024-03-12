using Microsoft.AspNetCore.Mvc;
using OLab.Api.Dto;

namespace OLab.Api.Data.Interface;

public interface IOLabAuthorization
{
  IActionResult HasAccess(string acl, ScopedObjectDto dto);
  bool HasAccess(string acl, string objectType, uint? objectId);
  IUserContext UserContext { get; set; }
  void ApplyUserContext(IUserContext userContext);
}