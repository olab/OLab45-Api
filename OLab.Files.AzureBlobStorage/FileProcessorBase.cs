using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using OLab.Common.Interfaces;
using SharpCompress.Archives;

namespace OLab.Files.AzureBlobStorage;

public abstract class FileProcessorBase : IFileProcessor
{
  private readonly BlobContainerClient _containerClient;
  public readonly IOLabLogger Logger;
  private readonly IOLabConfiguration _configuration;

  protected FileProcessorBase(
    BlobContainerClient containerClient,
    IOLabLogger logger,
    IOLabConfiguration configuration)
  {
    _containerClient = containerClient;
    _configuration = configuration;
    Logger = logger;
  }

  public abstract Task ProcessFileAsync(
    string archiveFileName, 
    string extractDirectory,
    Stream blobStream,
    CancellationToken token);

  protected async Task ExtractArchiveFiles(
    string extractDirectory,
    IEnumerable<IArchiveEntry> archiveEntries,
    CancellationToken token)
  {
    foreach (var archiveEntry in archiveEntries.Where(entry => !entry.IsDirectory))
    {
      var targetFile = $"{extractDirectory}/{archiveEntry.Key}";

      await using var fileStream = archiveEntry.OpenEntryStream();
      await _containerClient.UploadBlobAsync(targetFile, fileStream, token);

      Logger.LogInformation(
          $"Processed '{archiveEntry.Key}' -> '{targetFile}'");
    }
  }
}