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

  Task<IActionResult> HasAccessAsync(ulong acl, ScopedObjectDto dto);
  Task<bool> HasAccessAsync(ulong acl, string objectType, uint? objectId);
  bool HasAccess(ulong requestedPerm, string operationType);

  IUserContext UserContext { get; set; }
  void ApplyUserContext(IUserContext userContext);
  Task<bool> IsSystemSuperuserAsync();
  Task<bool> IsSuperUserInGroup(uint groupId);
}