using OLab.Api.Model;

namespace OLab.Test.DbContextTests;

public class SystemFilesDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<SystemFiles>( 2 );
    expecteds[ 0 ].Shared = 0;
    expecteds[ 0 ].Private = 0;
    expecteds[ 1 ].Shared = 1;
    expecteds[ 1 ].Private = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.SystemFiles.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.Shared == 1 )
        Assert.True( actual.IsShared );
      else
        Assert.False( actual.IsShared );
    }

    actuals[ 0 ].IsShared = true;
    Assert.True( actuals[ 0 ].Shared == 1 );
    Assert.True( actuals[ 0 ].Shared == 1 );

    actuals[ 0 ].IsPrivate = true;
    Assert.True( actuals[ 0 ].Private == 1 );
    Assert.True( actuals[ 0 ].Private == 1 );

    actuals[ 0 ].IsShared = false;
    Assert.True( actuals[ 0 ].Shared == 0 );
    Assert.True( actuals[ 0 ].Shared == 0 );

    actuals[ 0 ].IsPrivate = false;
    Assert.True( actuals[ 0 ].Private == 0 );
    Assert.True( actuals[ 0 ].Private == 0 );
  }

}