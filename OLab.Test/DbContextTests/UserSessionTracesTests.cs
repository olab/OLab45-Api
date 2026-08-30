using OLab.Api.Model;
using OLab.Test;

namespace OLab.Api.Tests;

public class UserSessionTraceDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<UserSessiontraces>( 2 );
    expecteds[ 0 ].Redirected = 0;
    expecteds[ 1 ].Redirected = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.UserSessiontraces.ToList();

    foreach ( var record in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == record.Id )
        ?? throw new Exception( "Record not found" );
      if ( actual.Redirected == 1 )
        Assert.True( actual.IsRedirected );
      else
        Assert.False( actual.IsRedirected );
    }

    actuals[ 0 ].IsRedirected = true;
    Assert.True( actuals[ 0 ].Redirected == 1 );

    actuals[ 0 ].IsRedirected = false;
    Assert.True( actuals[ 0 ].Redirected == 0 );
  }

}