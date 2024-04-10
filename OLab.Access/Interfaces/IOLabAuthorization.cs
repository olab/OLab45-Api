using Microsoft.AspNetCore.Mvc;
using OLab.Api.Dto;

namespace OLab.Api.Data.Interface;

public interface IOLabAuthorization
{
  IActionResult HasAccess(ulong acl, ScopedObjectDto dto);
  bool HasAccess(ulong acl, string objectType, uint? objectId);
  bool IsMemberOf(string groupName, string RoleName);

  IUserContext UserContext { get; set; }
  void ApplyUserContext(IUserContext userContext);
}