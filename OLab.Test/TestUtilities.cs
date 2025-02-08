using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Api.Model;

namespace OLab.Test;
internal static class TestUtilities
{
  public static IQueryable<T> LoadRecordsFromJson<T>(string filePath)
  {
    if ( !File.Exists( filePath ) )
      throw new FileNotFoundException( $"File not found: {filePath}" );

    var json = File.ReadAllText( filePath );
    var data = JsonConvert.DeserializeObject<List<T>>( json );
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }

  public static IList<GrouproleAcls> LoadGroupRoleAclFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<GrouproleAcls>( v );
    mockDbContext.Setup( x => x.GrouproleAcls ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Maps> LoadMapFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Maps>( v );
    mockDbContext.Setup( x => x.Maps ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Groups> LoadGroupFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Groups>( v );
    mockDbContext.Setup( x => x.Groups ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Roles> LoadRoleFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<Roles>( v );
    mockDbContext.Setup( x => x.Roles ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<SystemApplications> LoadSystemApplicationsFromJson(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadRecordsFromJson<SystemApplications>( v );
    mockDbContext.Setup( x => x.SystemApplications).ReturnsDbSet( list );
    return list.ToList();
  }
}
