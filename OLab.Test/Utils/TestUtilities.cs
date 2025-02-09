using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Api.Model;

namespace OLab.Test.Utils;
internal static class TestUtilities
{
  public static IQueryable<T> LoadObjectFromJson<T>(string filePath)
  {
    if ( !File.Exists( filePath ) )
      throw new FileNotFoundException( $"File not found: {filePath}" );

    var json = File.ReadAllText( filePath );
    var data = JsonConvert.DeserializeObject<List<T>>( json );
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }

  public static IList<GrouproleAcls> MoqGroupRoleAclFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<GrouproleAcls>( v );
    mockDbContext.Setup( x => x.GrouproleAcls ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Maps> MoqMapFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<Maps>( v );
    mockDbContext.Setup( x => x.Maps ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Groups> MoqGroupsFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<Groups>( v );
    mockDbContext.Setup( x => x.Groups ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Roles> MoqRoleFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<Roles>( v );
    mockDbContext.Setup( x => x.Roles ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<SystemApplications> MoqSystemApplicationsFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<SystemApplications>( v );
    mockDbContext.Setup( x => x.SystemApplications ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Users> MoqUsersFromJson(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = LoadObjectFromJson<Users>( v );
    mockDbContext.Setup( x => x.Users ).ReturnsDbSet( list );
    return list.ToList();
  }
}
