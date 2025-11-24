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
  public void GetAll_ReturnsAllRecords_WithVisibilitySetProperly()
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
        Assert.True( actual.IsVisible );
      else
        Assert.False( actual.IsVisible );
    }

    actuals[ 0 ].IsVisible = true;
    Assert.True( actuals[ 0 ].Visible == 1 );

    actuals[ 0 ].IsVisible = false;
    Assert.True( actuals[ 0 ].Visible == 0 );
  }

}