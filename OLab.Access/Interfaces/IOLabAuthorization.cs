using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Dto;
using OLab.Api.Model;
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
  Task<bool> HasAccessAsync(ulong requestedPerm, string operationType);

  IUserContext UserContext { get; set; }
  void ApplyUserContext(IUserContext userContext);
  Task<bool> IsSystemSuperuserAsync();
  Task<bool> IsGroupSuperUserAsync(uint groupId);
  Task<bool> HasAccessToAppAsync(Users userPhys, string appName);

}