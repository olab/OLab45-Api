using OLab.Api.Model;

namespace OLab.Test;

public class SystemCounterActionsTests
{
  [Fact]
  public void ApplyFunctionToCounter_WithStringValue_ProcessesCorrectly()
  {
    var counter = new SystemCounters { Value = System.Text.Encoding.UTF8.GetBytes( "oldValue" ) };
    var action = new SystemCounterActions { Expression = "=newValue" };

    var result = action.ApplyFunctionToCounter( counter );

    Assert.True( result );
    Assert.Equal( "newValue", counter.ValueAsString() );
  }

  [Fact]
  public void ProcessStringCounter_WithValidExpression_UpdatesCounter()
  {
    var counter = new SystemCounters { Value = System.Text.Encoding.UTF8.GetBytes( "oldValue" ) };
    var action = new SystemCounterActions { Expression = "=newValue" };

    var result = action.ProcessStringCounter( counter );

    Assert.True( result );
    Assert.Equal( "newValue", counter.ValueAsString() );
  }

}
