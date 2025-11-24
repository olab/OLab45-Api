using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OLab.Test.DbContextTests;

public class LrsDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithVisibilitySetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<Lrs>( 2 );
    expecteds[0].Enabled = 0;
    expecteds[1].Enabled = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.Lrs.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.Enabled == 1 )
        Assert.True( actual.IsEnabled );
      else
        Assert.False( actual.IsEnabled );
    }

  }

}