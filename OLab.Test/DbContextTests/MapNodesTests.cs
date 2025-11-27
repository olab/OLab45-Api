using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OLab.Test.DbContextTests;

public class MapNodesDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<MapNodes>( 2 );
    expecteds[ 0 ].Probability = 0;
    expecteds[ 0 ].Kfp = 0;
    expecteds[ 0 ].Undo = 0;
    expecteds[ 0 ].End = 0;
    expecteds[ 0 ].ShowInfo = 0;
    expecteds[ 1 ].Probability = 1;
    expecteds[ 1 ].Kfp = 1;
    expecteds[ 1 ].Undo = 1;
    expecteds[ 1 ].End = 1;
    expecteds[ 1 ].ShowInfo = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.MapNodes.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );

      if ( expected.Probability == 1 )
        Assert.True( actual.IsProbability );
      else
        Assert.False( actual.IsProbability );

      if ( expected.Kfp == 1 )
        Assert.True( actual.IsKfp );
      else
        Assert.False( actual.IsKfp );

      if ( expected.End == 1 )
        Assert.True( actual.IsEnd);
      else
        Assert.False( actual.IsEnd);

      if ( expected.ShowInfo == 1 )
        Assert.True( actual.IsShowInfo );
      else
        Assert.False( actual.IsShowInfo );

    }

    actuals[ 0 ].IsProbability = true;
    Assert.True( actuals[ 0 ].Probability == 1 );

    actuals[ 0 ].IsProbability = false;
    Assert.True( actuals[ 0 ].Probability == 0 );

    actuals[ 0 ].IsKfp = true;
    Assert.True( actuals[ 0 ].Kfp == 1 );

    actuals[ 0 ].IsKfp = false;
    Assert.True( actuals[ 0 ].Kfp == 0 );

    actuals[ 0 ].IsEnd = true;
    Assert.True( actuals[ 0 ].End == 1 );

    actuals[ 0 ].IsEnd = false;
    Assert.True( actuals[ 0 ].End == 0 );

    actuals[ 0 ].IsShowInfo = true;
    Assert.True( actuals[ 0 ].ShowInfo == 1 );

    actuals[ 0 ].IsShowInfo = false;
    Assert.True( actuals[ 0 ].ShowInfo == 0 );
  }

}