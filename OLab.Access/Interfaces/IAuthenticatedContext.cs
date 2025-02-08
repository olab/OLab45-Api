using OLab.Api.Model;
using System.Collections.Generic;

namespace OLab.Access.Interfaces;

public interface IAuthenticatedContext
{
  public string SessionId
  {
    get;
    set;
  }

  public IList<UserGrouproles> GroupRoles
  {
    get;
    set;
  }

  public uint UserId
  {
    get;
    set;
  }

  public string UserName
  {
    get;
    set;
  }

  public string IPAddress
  {
    get;
    set;
  }
  public string Issuer
  {
    get;
    set;
  }

  string AppName
  {
    get;
    set;
  }

  string ReferringCourse
  {
    get;
    set;
  }

  IDictionary<string, string> Claims { get; }

  //public IList<string> UserRoles { get; }

  public string ToString();
}