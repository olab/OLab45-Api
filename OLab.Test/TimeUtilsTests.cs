using OLab.Common.Utils;

namespace OLab.Test
{
  public class TimeUtilsTests
  {
    [Fact]
    public void ToUtc_WithNullSource_ReturnsNull()
    {
      DateTime? source = null;
      var result = TimeUtils.ToUtc( source );
      Assert.Null( result );
    }

    [Fact]
    public void ToUtc_WithUtcSource_ReturnsSameValue()
    {
      DateTime? source = new DateTime( 2023, 10, 1, 0, 0, 0, DateTimeKind.Utc );
      var result = TimeUtils.ToUtc( source );
      Assert.Equal( source, result );
    }

    [Fact]
    public void ToUtc_WithNonUtcSource_ReturnsUtcValue()
    {
      DateTime? source = new DateTime( 2023, 10, 1, 0, 0, 0, DateTimeKind.Local );
      var result = TimeUtils.ToUtc( source );
      var expected = DateTime.SpecifyKind( source.Value, DateTimeKind.Utc );
      Assert.Equal( expected, result );
    }
  }
}
