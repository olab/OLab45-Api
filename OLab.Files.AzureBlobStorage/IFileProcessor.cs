namespace OLab.Files.AzureBlobStorage;

public interface IFileProcessor
{
  Task ProcessFileAsync(
    string archiveFileName,
    string extractDirectory,
    Stream blobStream,
    CancellationToken token);
}
