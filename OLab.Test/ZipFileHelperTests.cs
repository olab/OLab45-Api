using Moq;
using OLab.Import;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
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

    var writerOptions = new ZipWriterOptions( CompressionType.Deflate )
    {
      // No DeflateCompressionLevel property in 0.48.0
      // Everything else stays default
    };

    using ( var writer = WriterFactory.OpenWriter( memoryStream, ArchiveType.Zip, writerOptions ) )
    {
      foreach ( var file in files )
      {
        var data = Encoding.UTF8.GetBytes( file.Value );
        using var fileStream = new MemoryStream( data );

        writer.Write( file.Key, fileStream );
      }
    }

    memoryStream.Position = 0;
    return memoryStream;
  }



}
