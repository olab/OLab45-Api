using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OLab.Test.DbContextTests;

public class MapsDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<Maps>( 2 );
    expecteds[ 0 ].Timing = 0;
    expecteds[ 0 ].ShowBar = 0;
    expecteds[ 0 ].ShowScore = 0;
    expecteds[ 0 ].Enabled = 0;
    expecteds[ 0 ].RevisableAnswers = 0;
    expecteds[ 0 ].SendXapiStatements = 0;
    expecteds[ 1 ].Timing = 1;
    expecteds[ 1 ].ShowBar = 1;
    expecteds[ 1 ].ShowScore = 1;
    expecteds[ 1 ].Enabled = 1;
    expecteds[ 1 ].RevisableAnswers = 1;
    expecteds[ 1 ].SendXapiStatements = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.Maps.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );

      if ( expected.Timing == 1 )
        Assert.True( actual.IsTiming );
      else
        Assert.False( actual.IsTiming );

      if ( expected.ShowBar == 1 )
        Assert.True( actual.IsShowBar );
      else
        Assert.False( actual.IsShowBar );

      if ( expected.Enabled == 1 )
        Assert.True( actual.IsEnabled);
      else
        Assert.False( actual.IsEnabled);

      if ( expected.RevisableAnswers == 1 )
        Assert.True( actual.IsRevisableAnswers );
      else
        Assert.False( actual.IsRevisableAnswers );

      if ( expected.SendXapiStatements == 1 )
        Assert.True( actual.IsSendXapiStatements );
      else
        Assert.False( actual.IsSendXapiStatements );
    }

    actuals[ 0 ].IsTiming = true;
    Assert.True( actuals[ 0 ].Timing == 1 );

    actuals[ 0 ].IsTiming = false;
    Assert.True( actuals[ 0 ].Timing == 0 );

    actuals[ 0 ].IsShowBar = true;
    Assert.True( actuals[ 0 ].ShowBar == 1 );

    actuals[ 0 ].IsShowBar = false;
    Assert.True( actuals[ 0 ].ShowBar == 0 );

    actuals[ 0 ].IsEnabled = true;
    Assert.True( actuals[ 0 ].Enabled == 1 );

    actuals[ 0 ].IsEnabled = false;
    Assert.True( actuals[ 0 ].Enabled == 0 );

    actuals[ 0 ].IsRevisableAnswers = true;
    Assert.True( actuals[ 0 ].RevisableAnswers == 1 );

    actuals[ 0 ].IsRevisableAnswers = false;
    Assert.True( actuals[ 0 ].RevisableAnswers == 0 );

    actuals[ 0 ].IsSendXapiStatements = true;
    Assert.True( actuals[ 0 ].SendXapiStatements == 1 );

    actuals[ 0 ].IsSendXapiStatements = false;
    Assert.True( actuals[ 0 ].SendXapiStatements == 0 );
  }

}