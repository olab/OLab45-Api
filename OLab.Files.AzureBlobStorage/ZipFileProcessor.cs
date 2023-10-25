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
    IOLabConfiguration configuration) : base(containerClient, logger, configuration)
  {
  }

  public override async Task ProcessFileAsync(
    string archiveFileName,
    string extractDirectory,
    Stream blobStream,
    CancellationToken token)
  {
    if (ZipArchive.IsZipFile(blobStream))
    {
      var zipReaderOptions = new ReaderOptions()
      {
        ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8),
        LookForHeader = true
      };

      Logger.LogInformation("Blob is a zip file; beginning extraction....");
      blobStream.Position = 0;

      using var reader = ZipArchive.Open(blobStream, zipReaderOptions);
      await ExtractArchiveFiles(extractDirectory, reader.Entries, token);
    }
  }
}