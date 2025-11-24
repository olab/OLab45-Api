using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Xunit;

namespace OLab.Api.Tests;

public class UserSessionsDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<UserSessions>( 2 );
    expecteds[ 0 ].NotCumulative = 0;
    expecteds[ 1 ].NotCumulative = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.UserSessions.ToList();

    foreach ( var record in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == record.Id )
        ?? throw new Exception( "Record not found" );
      if ( actual.NotCumulative == 1 )
        Assert.True( actual.IsNotCumulative );
      else
        Assert.True( actual.IsNotCumulative );
    }

    actuals[ 0 ].IsNotCumulative = true;
    Assert.True( actuals[ 0 ].NotCumulative == 1 );

    actuals[ 0 ].IsNotCumulative = false;
    Assert.True( actuals[ 0 ].NotCumulative == 0 );
  }

}