using Moq;
using OLab.Import;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.IO;
using System.Text;
using Xunit;

namespace OLab.Test;

public class ZipFileHelperTests
{
  [Fact]
  public void GetFiles_WithValidZipStream_ReturnsFileList()
  {
    // Arrange
    var zipStream = CreateTestZipStream( new Dictionary<string, string>
        {
          { "file1.txt", "content1" },
          { "file2.txt", "content2" }
        } );

    // Act
    var result = ZipFileHelper.GetFiles( zipStream );

    // Assert
    Assert.Equal( 2, result.Count );
    Assert.Contains( "file1.txt", result );
    Assert.Contains( "file2.txt", result );
  }

  [Fact]
  public void GetFiles_WithEmptyZipStream_ReturnsEmptyList()
  {
    // Arrange
    var zipStream = CreateTestZipStream( new Dictionary<string, string>() );

    // Act
    var result = ZipFileHelper.GetFiles( zipStream );

    // Assert
    Assert.Empty( result );
  }

  [Fact]
  public void GetFileEntries_WithValidZipStream_ReturnsEntryList()
  {
    // Arrange
    var zipStream = CreateTestZipStream( new Dictionary<string, string>
        {
          { "file1.txt", "content1" },
          { "file2.txt", "content2" }
        } );

    // Act
    var result = ZipFileHelper.GetFileEntries( zipStream );

    // Assert
    Assert.Equal( 2, result.Count );
    Assert.Contains( result, entry => entry.Key == "file1.txt" );
    Assert.Contains( result, entry => entry.Key == "file2.txt" );
  }

  [Fact]
  public void GetFileEntries_WithEmptyZipStream_ReturnsEmptyList()
  {
    // Arrange
    var zipStream = CreateTestZipStream( new Dictionary<string, string>() );

    // Act
    var result = ZipFileHelper.GetFileEntries( zipStream );

    // Assert
    Assert.Empty( result );
  }

  private Stream CreateTestZipStream(Dictionary<string, string> files)
  {
    var memoryStream = new MemoryStream();
    using ( var archive = ZipArchive.Create() )
    {
      foreach ( var file in files )
      {
        var entry = archive.AddEntry( file.Key, new MemoryStream( Encoding.UTF8.GetBytes( file.Value ) ) );
      }
      archive.SaveTo( memoryStream, new WriterOptions( CompressionType.Deflate ) );
    }
    memoryStream.Position = 0;
    return memoryStream;
  }
}
