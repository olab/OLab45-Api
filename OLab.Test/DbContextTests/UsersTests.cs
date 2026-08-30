using OLab.Api.Model;

namespace OLab.Test.DbContextTests;

public class UsersDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<Users>( 2 );
    expecteds[ 0 ].HistoryReadonly = 0;
    expecteds[ 0 ].Lti = 0;
    expecteds[ 1 ].HistoryReadonly = 1;
    expecteds[ 1 ].Lti = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.Users.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );

      if ( expected.HistoryReadonly == 1 )
        Assert.True( actual.IsHistoryReadonly );
      else
        Assert.False( actual.IsHistoryReadonly );

      if ( expected.Lti == 1 )
        Assert.True( actual.IsLti );
      else
        Assert.False( actual.IsLti );

    }

    actuals[ 0 ].IsHistoryReadonly = true;
    Assert.True( actuals[ 0 ].HistoryReadonly == 1 );

    actuals[ 0 ].IsHistoryReadonly = false;
    Assert.True( actuals[ 0 ].HistoryReadonly == 0 );

    actuals[ 0 ].IsLti = true;
    Assert.True( actuals[ 0 ].Lti == 1 );

    actuals[ 0 ].IsLti = false;
    Assert.True( actuals[ 0 ].Lti == 0 );
  }

}