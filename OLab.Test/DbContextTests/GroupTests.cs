using OLab.Api.Model;
using OLab.Test;

namespace OLab.Api.Tests;

public class GroupDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<Groups>( 2 );
    expecteds[ 0 ].System = 0;
    expecteds[ 1 ].System = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.Groups.ToList();

    foreach ( var record in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == record.Id )
        ?? throw new Exception( "Record not found" );
      if ( actual.System == 1 )
        Assert.True( actual.IsSystem );
      else
        Assert.False( actual.IsSystem );
    }

    actuals[ 0 ].IsSystem = true;
    Assert.True( actuals[ 0 ].System == 1 );

    actuals[ 0 ].IsSystem = false;
    Assert.True( actuals[ 0 ].System == 0 );
  }

}