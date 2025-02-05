using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Test;
internal static class TestUtilities
{
  public static IQueryable<T> LoadRecordsFromJson<T>(string filePath)
  {
    var json = File.ReadAllText( filePath );
    var data = JsonConvert.DeserializeObject<List<T>>( json );
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }

  public static void LoadGroupRoleAclFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<GrouproleAcls>( v );

    var mockSet = new Mock<DbSet<GrouproleAcls>>();
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.Provider ).Returns( list.Provider );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.Expression ).Returns( list.Expression );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.ElementType ).Returns( list.ElementType );
    mockSet.As<IQueryable<GrouproleAcls>>().Setup( m => m.GetEnumerator() ).Returns( () => list.GetEnumerator() );

    mockDbContext.Setup( c => c.GrouproleAcls ).Returns( mockSet.Object );
  }

  internal static void LoadMapFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Maps>( v );
    mockDbContext.Setup( x => x.Maps ).ReturnsDbSet( list );
  }

  internal static void LoadGroupFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Groups>( v );
    mockDbContext.Setup( x => x.Groups ).ReturnsDbSet( list );
  }

  internal static void LoadRoleFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Roles>( v );
    mockDbContext.Setup( x => x.Roles ).ReturnsDbSet( list );
  }

  internal static void LoadSystemApplicationsFromJson(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<SystemApplications>( v );
    mockDbContext.Setup( x => x.SystemApplications).ReturnsDbSet( list );
  }
}
