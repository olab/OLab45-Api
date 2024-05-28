using Microsoft.AspNetCore.Mvc;
using OLab.Api.Dto;
using System.Threading.Tasks;

namespace OLab.Api.Data.Interface;

public interface IOLabAuthorization
{
  public const ulong AclBitMaskRead = 4;
  public const ulong AclBitMaskWrite = 2;
  public const ulong AclBitMaskExecute = 1;
  public const ulong AclBitMaskNoAccess = 0;

  IActionResult HasAccess(ulong acl, ScopedObjectDto dto);
  bool HasAccess(ulong acl, string objectType, uint? objectId);
  IUserContext UserContext { get; set; }
  void ApplyUserContext(IUserContext userContext);
  Task<bool> IsSystemSuperuserAsync();
  Task<bool> IsGroupSuperuserAsync(uint groupId);
}