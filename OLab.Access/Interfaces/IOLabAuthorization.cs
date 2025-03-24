using OLab.Api.Dto;
using OLab.Api.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OLab.Access.Interfaces;

public interface IOLabAuthorization
{
  public const ulong AclBitMaskFull = 7;
  public const ulong AclBitMaskRead = 4;
  public const ulong AclBitMaskWrite = 2;
  public const ulong AclBitMaskExecute = 1;
  public const ulong AclBitMaskNoAccess = 0;

  IAuthenticatedContext AuthenticatedContext { get; set; }
  IList<GrouproleAcls> GroupRoleAcls { get; }
  IList<UserGrouproles> UsersGroupRoles { get; }
  string ExtractApplicationFromUri(string requestUri);
  string Issuer { get; set; }
  Task ApplyUserContextAsync(IAuthenticatedContext userContext);

  Task<bool> HasAccessAsync(ulong acl, MapNodes dto);
  Task<bool> HasAccessAsync(ulong acl, Maps dto);
  Task<bool> HasAccessAsync(ulong acl, ScopedObjectDto dto);
  Task<bool> HasAccessToAppAsync(Users userPhys, string appName);
  Task<IList<Groups>> GetAuthorizedUserGroupsAsync();

  Task<bool> HasAccessAsync(ulong aclBitMaskRead, string scopeLevel, uint id);

  Task<bool> IsGroupSuperUserAsync(uint groupId);
  Task<bool> IsSystemSuperuserAsync();
  Task<MapGrouproles> GetMapCreationGroupRoleAsync(Maps map);
  Users OLabUser { get; set; }

}