using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using OLab.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OLab.Test.DbContextTests;

public class SystemQuestionsDBTests
{
  [Fact]
  public void GetAll_ReturnsAllRecords_WithSbytePropertiesSetProperly()
  {
    var expecteds = OlabDbContextTest.CreateMany<SystemQuestions>( 2 );
    expecteds[0].ShowAnswer = 0;
    expecteds[0].ShowSubmit = 0;
    expecteds[0].ShowSubmit = 0;
    expecteds[1].ShowSubmit = 1;

    var mockContext = OlabDbContextTest.CreateMockDbContextWithDbSet( expecteds );
    var actuals = mockContext.Object.SystemQuestions.ToList();

    foreach ( var expected in expecteds )
    {
      var actual = actuals.FirstOrDefault( r => r.Id == expected.Id ) ?? throw new Exception( "Record not found" );
      if ( expected.ShowAnswer == 1 )
        Assert.True( actual.IsShowAnswer );
      else
        Assert.False( actual.IsShowAnswer );

      if ( expected.ShowSubmit == 1 )
        Assert.True( actual.IsShowSubmit );
      else
        Assert.False( actual.IsShowSubmit );
    }

    actuals[ 0 ].IsShowAnswer = true;
    Assert.True( actuals[ 0 ].ShowAnswer == 1 );

    actuals[ 0 ].IsShowAnswer = false;
    Assert.True( actuals[ 0 ].ShowAnswer == 0 );

    actuals[ 0 ].IsShowSubmit = true;
    Assert.True( actuals[ 0 ].ShowSubmit == 1 );

    actuals[ 0 ].IsShowSubmit = false;
    Assert.True( actuals[ 0 ].ShowSubmit == 0 );
  }

}