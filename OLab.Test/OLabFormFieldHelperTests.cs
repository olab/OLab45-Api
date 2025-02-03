using OLab.Common.Utils;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace OLab.Test
{
  public class OLabFormFieldHelperTests
  {
    [Fact]
    public void Constructor_WithValidStream_InitializesFields()
    {
      using var stream = new MemoryStream();
      var helper = new OLabFormFieldHelper( stream );

      Assert.NotNull( helper.Fields );
      Assert.Empty( helper.Fields );
      Assert.Equal( stream, helper.Stream );
    }

    [Fact]
    public void Field_WithExistingKey_ReturnsValue()
    {
      using var stream = new MemoryStream();
      var helper = new OLabFormFieldHelper( stream );
      helper.Fields[ "testKey" ] = "testValue";

      var result = helper.Field( "testKey" );

      Assert.Equal( "testValue", result );
    }

    [Fact]
    public void Field_WithNonExistingKey_ReturnsEmptyString()
    {
      using var stream = new MemoryStream();
      var helper = new OLabFormFieldHelper( stream );

      var result = helper.Field( "nonExistingKey" );

      Assert.Equal( string.Empty, result );
    }
  }
}
