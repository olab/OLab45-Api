using OLab.Common.Utils;

namespace OLab.Test;

public class ConversionsTests
{
  [Fact]
  public void OptionalIdSafeAssign_WithNonZeroValue_ReturnsValue()
  {
    uint source = 5;
    var result = Conversions.OptionalIdSafeAssign( source );
    Assert.Equal( source, result );
  }

  [Fact]
  public void OptionalIdSafeAssign_WithZeroValue_ReturnsNull()
  {
    uint source = 0;
    var result = Conversions.OptionalIdSafeAssign( source );
    Assert.Null( result );
  }

  [Fact]
  public void OptionalIdSafeAssign_WithNullableNonZeroValue_ReturnsValue()
  {
    uint? source = 5;
    var result = Conversions.OptionalIdSafeAssign( source );
    Assert.Equal( source, result );
  }

  [Fact]
  public void OptionalIdSafeAssign_WithNullableZeroValue_ReturnsNull()
  {
    uint? source = 0;
    var result = Conversions.OptionalIdSafeAssign( source );
    Assert.Null( result );
  }

  [Fact]
  public void OptionalIdSafeAssign_WithNullableNullValue_ReturnsNull()
  {
    uint? source = null;
    var result = Conversions.OptionalIdSafeAssign( source );
    Assert.Null( result );
  }

  [Fact]
  public void Base64Decode_WithValidBase64_ReturnsDecodedString()
  {
    var source = "SGVsbG8gd29ybGQ=";
    var result = Conversions.Base64Decode( source );
    Assert.Equal( "Hello world", result );
  }

  [Fact]
  public void Base64Decode_WithInvalidBase64_ReturnsSourceString()
  {
    var source = "InvalidBase64";
    var result = Conversions.Base64Decode( source, false );
    Assert.Equal( source, result );
  }

  [Fact]
  public void GetCurrentUnixTime_ReturnsCurrentUnixTime()
  {
    var result = Conversions.GetCurrentUnixTime();
    var expected = (decimal)(DateTime.UtcNow - new DateTime( 1970, 1, 1 )).TotalSeconds;
    Assert.True( Math.Abs( result - expected ) < 1 );
  }

  [Fact]
  public void GetTime_WithValidEpochSeconds_ReturnsDateTime()
  {
    decimal epochSeconds = 1633072800; // 2021-10-01 00:00:00 UTC
    var result = Conversions.GetTime( epochSeconds );
    var expected = new DateTime( 2021, 10, 1, 7, 20, 0, DateTimeKind.Utc );
    Assert.Equal( expected, result );
  }
}
