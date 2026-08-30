using OLab.Api.Model;
using OLab.Test;

namespace OLab.Api.Tests;

public class SystemScriptsDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<SystemScripts>( 2 );
    expecteds[ 0 ].Raw = 0;
    expecteds[ 1 ].Raw = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.SystemScripts.ToList();

    foreach ( var record in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == record.Id )
        ?? throw new Exception( "Record not found" );
      if ( actual.Raw == 1 )
        Assert.True( actual.IsRaw );
      else
        Assert.False( actual.IsRaw );
    }

    actuals[ 0 ].IsRaw = true;
    Assert.True( actuals[ 0 ].Raw == 1 );

    actuals[ 0 ].IsRaw = false;
    Assert.True( actuals[ 0 ].Raw == 0 );
  }

}