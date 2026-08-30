using OLab.Api.Model;

namespace OLab.Test.DbContextTests;

public class MapNodeLinksDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<MapNodeLinks>( 2 );
    expecteds[ 0 ].Hidden = 0;
    expecteds[ 1 ].Hidden = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.MapNodeLinks.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.Hidden == 1 )
        Assert.True( actual.IsHidden );
      else
        Assert.False( actual.IsHidden );
    }

    actuals[ 0 ].IsHidden = true;
    Assert.True( actuals[ 0 ].Hidden == 1 );

    actuals[ 0 ].IsHidden = false;
    Assert.True( actuals[ 0 ].Hidden == 0 );

  }

}