using Microsoft.AspNetCore.Mvc;
using OLab.Api.Dto;
using OLab.Api.Model;
using System;
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

  Users OLabUser { get; set; }
  IUserContext UserContext { get; set; }

  string Issuer { get; set; }
  Task ApplyUserContextAsync(IUserContext userContext);
  Task<bool> IsSystemSuperuserAsync();
  Task<bool> IsGroupSuperUserAsync(uint groupId);
  Task<bool> HasAccessToAppAsync(Users userPhys, string appName);
  Task<MapGrouproles> GetMapCreationGroupRoleAsync(Maps map);
  string ExtractApplication(string refererValue);


}