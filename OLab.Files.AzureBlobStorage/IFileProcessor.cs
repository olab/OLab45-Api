namespace OLab.Files.AzureBlobStorage;

public interface IFileProcessor
{
  Task ProcessFileAsync(
    Stream stream,
    string extractDirectory,
    CancellationToken token);
}
