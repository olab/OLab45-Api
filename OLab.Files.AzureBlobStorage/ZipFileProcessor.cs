using Azure.Storage.Blobs;
using OLab.Common.Interfaces;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Text;

namespace OLab.Files.AzureBlobStorage;

public class ZipFileProcessor : FileProcessorBase
{
  public ZipFileProcessor(
    BlobContainerClient containerClient,
    IOLabLogger logger,
    IOLabConfiguration configuration) : base( containerClient, logger, configuration )
  {
  }

  public override async Task ProcessFileAsync(
    Stream stream,
    string extractDirectory,
    CancellationToken token)
  {
    if ( ZipArchive.IsZipFile( stream ) )
    {
      var zipReaderOptions = new ReaderOptions
      {
        ArchiveEncoding = new ArchiveEncoding
        {
          Default = Encoding.UTF8,
          Forced = Encoding.UTF8
        },
        LookForHeader = true
      };

      Logger.LogInformation( "Blob is a zip file; beginning extraction...." );
      stream.Position = 0;

      using var reader = ReaderFactory.OpenReader( stream, zipReaderOptions );

      var extractionOptions = new ExtractionOptions
      {
        ExtractFullPath = true,
        Overwrite = true
      };

      while ( reader.MoveToNextEntry() )
      {
        if ( token.IsCancellationRequested )
          break;

        if ( !reader.Entry.IsDirectory )
        {
          reader.WriteEntryToDirectory( extractDirectory, extractionOptions );
        }
      }
    }

    await Task.CompletedTask;
  }
}
