using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OLab.Test.DbContextTests;

public class SystemCountersDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<SystemCounters>( 2 );
    expecteds[0].Visible = 0;
    expecteds[1].Visible = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.SystemCounters.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.Visible == 1 )
        Assert.True( actual.Visible == 1);
      else
        Assert.True( actual.Visible == 0 );
    }

    actuals[ 0 ].Visible = 1;
    Assert.True( actuals[ 0 ].Visible == 1 );

    actuals[ 0 ].Visible = 0;
    Assert.True( actuals[ 0 ].Visible == 0 );
  }

}