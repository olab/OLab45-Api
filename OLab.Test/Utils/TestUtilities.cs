using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Api.Model;

namespace OLab.Test.Utils;
internal static class TestUtilities
{

  public static IQueryable<T> BuildQueryableListFromList<T>(IList<T> data)
  {
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }

  public static IQueryable<T> BuildQueryableListFromJson<T>(string filePath)
  {
    if ( !File.Exists( filePath ) )
      throw new FileNotFoundException( $"File not found: {filePath}" );

    var json = File.ReadAllText( filePath );
    var data = JsonConvert.DeserializeObject<List<T>>( json );
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }

  public static IList<GrouproleAcls> MoqGroupRoleAclFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<GrouproleAcls>( v );
    var mockSet = MockDbSetHelper.CreateMockDbSet( list );

    mockDbContext.Setup( x => x.GrouproleAcls ).ReturnsDbSet( mockSet.Object );
    mockDbContext.Setup( x => x.Set<GrouproleAcls>()).ReturnsDbSet( mockSet.Object );

    return list.ToList();
  }

  internal static IList<Maps> MoqMapFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<Maps>( v );
    mockDbContext.Setup( x => x.Maps ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<Groups> MoqGroupsFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<Groups>( v );
    var mockSet = MockDbSetHelper.CreateMockDbSet( list );

    mockDbContext.Setup( x => x.Groups ).ReturnsDbSet( list );
    mockDbContext.Setup( x => x.Set<Groups>() ).ReturnsDbSet( mockSet.Object );

    return list.ToList();
  }

  internal static IList<Roles> MoqRoleFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<Roles>( v );
    var mockSet = MockDbSetHelper.CreateMockDbSet( list );

    mockDbContext.Setup( x => x.Roles ).ReturnsDbSet( list );
    mockDbContext.Setup( x => x.Set<Roles>() ).ReturnsDbSet( mockSet.Object );

    return list.ToList();
  }

  internal static IList<SystemApplications> MoqSystemApplicationsFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<SystemApplications>( v );
    mockDbContext.Setup( x => x.SystemApplications ).ReturnsDbSet( list );
    return list.ToList();
  }

  internal static IList<SystemQuestions> MoqSystemQuestionsFromJsonFile(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<SystemQuestions>( v );
    mockDbContext.Setup( x => x.SystemQuestions ).ReturnsDbSet( list );
    return list.ToList();
  }


  internal static IList<Users> MoqUsersFromJson(Mock<OLabDBContext> mockDbContext, string v)
  {
    var list = BuildQueryableListFromJson<Users>( v );
    mockDbContext.Setup( x => x.Users ).ReturnsDbSet( list );
    return list.ToList();
  }
}
