using OLab.Api.Model;

namespace OLab.Test.DbContextTests;

public class MapCounterCommonRulesDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<MapCounterCommonRules>( 2 );
    expecteds[ 0 ].Correct = 0;
    expecteds[ 1 ].Correct = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.MapCounterCommonRules.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.Correct == 1 )
        Assert.True( actual.IsCorrect );
      else
        Assert.False( actual.IsCorrect );
    }

    actuals[ 0 ].IsCorrect = true;
    Assert.True( actuals[ 0 ].Correct == 1 );

    actuals[ 0 ].IsCorrect = false;
    Assert.True( actuals[ 0 ].Correct == 0 );

  }

}