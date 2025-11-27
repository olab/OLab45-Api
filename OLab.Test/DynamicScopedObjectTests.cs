using OLab.Api.Dto;

namespace OLab.Test
{
  public class DynamicScopedObjectTests
  {
    [Fact]
    public void Constructor_InitializesCounters()
    {
      // Arrange & Act
      var dynamicScopedObject = new DynamicScopedObject();

      // Assert
      Assert.NotNull( dynamicScopedObject.Counters );
      Assert.Empty( dynamicScopedObject.Counters );
    }

    [Fact]
    public void Counters_SetAndGetValues()
    {
      // Arrange
      var dynamicScopedObject = new DynamicScopedObject();
      var counters = new List<CountersDto>
              {
                  new CountersDto { Id = 1, Name = "Counter1" },
                  new CountersDto { Id = 2, Name = "Counter2" }
              };

      // Act
      dynamicScopedObject.Counters = counters;

      // Assert
      Assert.Equal( 2, dynamicScopedObject.Counters.Count );
      Assert.Equal( "Counter1", dynamicScopedObject.Counters[ 0 ].Name );
      Assert.Equal( "Counter2", dynamicScopedObject.Counters[ 1 ].Name );
    }
  }
}
