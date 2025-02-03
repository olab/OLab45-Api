using OLab.Api.Utils;
using Xunit;

namespace OLab.Test
{
  public class StringUtilsTests
  {
    [Fact]
    public void GenerateCheckSum_WithValidString_ReturnsExpectedChecksum()
    {
      var input = "Hello";
      var expected = "7BA84";
      var result = StringUtils.GenerateCheckSum( input );
      Assert.Equal( expected, result );
    }

    [Fact]
    public void StripUnicode_WithUnicodeCharacters_RemovesUnicodeCharacters()
    {
      var input = "Hello, 世界";
      var expected = "Hello, ";
      var result = StringUtils.StripUnicode( input );
      Assert.Equal( expected, result );
    }

    [Fact]
    public void EncryptString_WithValidKeyAndPlainText_ReturnsEncryptedString()
    {
      var key = "0123456789abcdef";
      var plainText = "Hello world";
      var result = StringUtils.EncryptString( key, plainText );
      Assert.NotNull( result );
      Assert.NotEqual( plainText, result );
    }

    [Fact]
    public void DecryptString_WithValidKeyAndCipherText_ReturnsDecryptedString()
    {
      var key = "0123456789abcdef";
      var plainText = "Hello world";
      var cipherText = StringUtils.EncryptString( key, plainText );
      var result = StringUtils.DecryptString( key, cipherText );
      Assert.Equal( plainText, result );
    }

    [Fact]
    public void EncryptString_DecryptString_WithValidKey_ReturnsOriginalString()
    {
      var key = "0123456789abcdef";
      var plainText = "Hello world";
      var encrypted = StringUtils.EncryptString( key, plainText );
      var decrypted = StringUtils.DecryptString( key, encrypted );
      Assert.Equal( plainText, decrypted );
    }
  }
}
