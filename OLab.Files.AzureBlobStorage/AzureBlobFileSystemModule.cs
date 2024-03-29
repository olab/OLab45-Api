using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dawn;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System.Configuration;
using System.IO.Compression;

namespace OLab.Files.AzureBlobStorage;

[OLabModule("AZUREBLOBSTORAGE")]
public class AzureBlobFileSystemModule : OLabFileStorageModule
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly string _containerName;

  private readonly Dictionary<string, IList<BlobItem>>
    _folderContentCache = new();

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="logger">OlabLogger</param>
  /// <param name="configuration">Application cfg</param>
  /// <exception cref="ConfigurationErrorsException"></exception>
  public AzureBlobFileSystemModule(
    IOLabLogger logger,
    IOLabConfiguration configuration) : base(logger, configuration)
  {
    // if not set to use this module, then don't proceed further
    if (GetModuleName().ToLower() != cfg.GetAppSettings().FileStorageType.ToLower())
      return;

    logger.LogInformation($"Initializing AzureBlobFileSystemModule");

    var connectionString = cfg.GetAppSettings().FileStorageConnectionString;
    if (string.IsNullOrEmpty(connectionString))
      throw new ConfigurationErrorsException("missing FileStorageConnectionString parameter");
    _blobServiceClient = new BlobServiceClient(connectionString);

    _containerName = Path.GetDirectoryName(cfg.GetAppSettings().FileStorageRoot);
    if (string.IsNullOrEmpty(_containerName))
      throw new ConfigurationErrorsException("missing FileStorageRoot parameter");

    logger.LogInformation($"Container: {_containerName}");

    // need to prevent container name from being part of the file root
    cfg.GetAppSettings().FileStorageRoot = Path.GetFileName(cfg.GetAppSettings().FileStorageRoot);
  }

  public override char GetFolderSeparator() { return '/'; }

  /// <summary>
  /// Test if a file exists
  /// </summary>
  /// <param name="filePath">Relative to root file path</param>
  /// <returns>true/false</returns>
  public override bool FileExists(
    string filePath)
  {
    Guard.Argument(filePath).NotEmpty(nameof(filePath));

    var result = false;

    try
    {
      IList<BlobItem> blobs;

      // if we do not have this sourceFolderName already in cache
      // then hit the blob storage and cache the results
      if (!_folderContentCache.ContainsKey(filePath))
      {
        logger.LogInformation($"  searching '{filePath} for blobs'");

        blobs = _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .GetBlobs(prefix: filePath).ToList();

        _folderContentCache[filePath] = blobs;

        foreach (var blob in blobs)
          logger.LogInformation($"  found blob '{blob.Name}'");

      }
      else
        blobs = _folderContentCache[filePath];

      result = blobs.Any(x => x.Name.Contains(Path.GetFileName(filePath)));

      if (!result)
        logger.LogWarning($"  '{filePath}' not found");
      else
        logger.LogInformation($"  '{filePath}' exists");

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "FileExists error");
      throw;
    }

    return result;
  }

  /// <summary>
  /// Move file between folders
  /// </summary>
  /// <param name="sourceFilePath">Relative to root file path</param>
  /// <param name="destinationFolder">Relative to root destination folder name</param>
  public override async Task MoveFileAsync(
    string sourceFilePath,
    string destinationFolder,
    CancellationToken token = default)
  {
    Guard.Argument(sourceFilePath).NotEmpty(nameof(sourceFilePath));
    Guard.Argument(destinationFolder).NotEmpty(nameof(destinationFolder));

    try
    {
      logger.LogInformation($"MoveFileAsync '{sourceFilePath} -> {destinationFolder}");

      using var stream = new MemoryStream();

      await ReadFileAsync(stream, sourceFilePath, token);
      await WriteFileAsync(
        stream,
        BuildPath(
          destinationFolder, 
          Path.GetFileName(sourceFilePath)), 
        token);
      await DeleteFileAsync(sourceFilePath);

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "MoveFileAsync error");
      throw;
    }
  }

  /// <summary>
  /// Copy file presented by stream to file store
  /// </summary>
  /// <param name="stream">File stream</param>
  /// <param name="fileType">Target folderName</param>
  /// <param name="filePath">Target folderName</param>
  /// <param name="token"></param>
  public override async Task<string> WriteFileAsync(
    Stream stream,
    string filePath,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(filePath).NotEmpty(nameof(filePath));

    try
    {
      logger.LogInformation($"WriteFileAsync: {_containerName} {filePath}");

      await _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(filePath)
            .UploadAsync(stream, overwrite: true, token);

      return filePath;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "WriteFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Read file from storage into stream
  /// </summary>
  /// <param name="stream">File stream</param>
  /// <param name="filePath">Relative to root file path</param>
  /// <param name="token">CancellationToken</param>
  public override async Task ReadFileAsync(
    Stream stream,
    string filePath,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(filePath).NotEmpty(nameof(filePath));

    try
    {
      await _blobServiceClient
           .GetBlobContainerClient(_containerName)
           .GetBlobClient(filePath)
           .DownloadToAsync(stream);

      logger.LogInformation($"ReadFileAsync: {_containerName} {filePath}. File size: {stream.Length}");

      stream.Position = 0;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ReadFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Delete file from blob storage
  /// </summary>
  /// <param name="logger">OLabLogger</param>
  /// <param name="filePath">Relative to root file path</param>
  /// <returns></returns>
  public override async Task<bool> DeleteFileAsync(
    string filePath)
  {
    Guard.Argument(filePath).NotEmpty(nameof(filePath));

    try
    {
      var physicalFileName = filePath;
      logger.LogInformation($"DeleteFileAsync '{physicalFileName}'");

      await _blobServiceClient
        .GetBlobContainerClient(_containerName)
        .DeleteBlobAsync(physicalFileName);

      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "DeleteFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Delete folder from blob storage
  /// </summary>
  /// <param name="folderName">Folder to delete</param>
  public override async Task DeleteFolderAsync(
    string folderName)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));

    await DeleteImportFilesAsync(
      _blobServiceClient.GetBlobContainerClient(_containerName),
      GetPhysicalPath(folderName),
      null);
  }

  /// <summary>
  /// Extract a file to blob storage
  /// </summary>
  /// <param name="logger">OLabLogger</param>
  /// <param name="archiveFileName">Source file name</param>
  /// <param name="extractDirectory">TTarget extreaction sourceFolderName</param>
  /// <param name="token">Cancellation token</param>
  /// <returns></returns>
  public override async Task<bool> ExtractFileToStorageAsync(
    string archiveFileName,
    string extractDirectory,
    CancellationToken token = default)
  {
    Guard.Argument(archiveFileName).NotEmpty(nameof(archiveFileName));
    Guard.Argument(extractDirectory).NotEmpty(nameof(extractDirectory));

    try
    {
      logger.LogInformation($"extracting {archiveFileName} -> {extractDirectory}");

      using var stream = new MemoryStream();
      var fileProcessor = new ZipFileProcessor(
        _blobServiceClient.GetBlobContainerClient(_containerName),
        logger,
        cfg);

      await ReadFileAsync(
        stream,
        archiveFileName,
        token);

      await fileProcessor.ProcessFileAsync(
        stream,
        extractDirectory,
        token);

      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ExtractFileToStorageAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Create archive file from a folderName
  /// </summary>
  /// <param name="archive">Archive file stream</param>
  /// <param name="folderName">Source file folderName</param>
  /// <param name="appendToStream">Append or replace stream contents</param>
  /// <param name="token"></param>
  public override async Task<bool> CopyFolderToArchiveAsync(
    ZipArchive archive,
    string folderName,
    string zipEntryFolderName,
    bool appendToStream,
    CancellationToken token = default)
  {
    Guard.Argument(archive).NotNull(nameof(archive));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));

    var result = false;

    try
    {
      IList<BlobItem> blobs;

      var physicalFolder = GetPhysicalPath(folderName);
      logger.LogInformation($"reading '{physicalFolder}' for files to add to stream");

      blobs = _blobServiceClient
        .GetBlobContainerClient(_containerName)
        .GetBlobs(prefix: physicalFolder).ToList();

      foreach (var blob in blobs)
      {
        var blobStream = new MemoryStream();

        await _blobServiceClient
             .GetBlobContainerClient(_containerName)
             .GetBlobClient(blob.Name)
             .DownloadToAsync(blobStream);

        blobStream.Position = 0;

        var entryPath = BuildPath(zipEntryFolderName, Path.GetFileName(blob.Name));
        logger.LogInformation($"  adding '{blob.Name}' to archive '{entryPath}'. size = {blobStream.Length}");

        var entry = archive.CreateEntry(entryPath);
        using var entryStream = entry.Open();
        blobStream.CopyTo(entryStream);
        entryStream.Close();

      }

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyFolderToArchiveAsync error");
      throw;
    }


    return result;
  }

  /// <summary>
  /// Get files in folder
  /// </summary>
  /// <param name="folderName"></param>
  /// <param name="token"></param>
  /// <returns></returns>
  public override IList<string> GetFiles(
    string folderName,
    CancellationToken token = default)
  {
    var fileNames = new List<string>();

    try
    {
      logger.LogInformation($"looking in '{folderName}' for files");

      var blobs = _blobServiceClient
        .GetBlobContainerClient(_containerName)
        .GetBlobs(prefix: folderName).ToList();

      if (blobs.Count == 0)
        return fileNames;

      logger.LogInformation($"  found '{blobs.Count}' files");
      fileNames = blobs.Select(blob => Path.GetFileName(blob.Name)).ToList();

      foreach (var fileName in fileNames)
        logger.LogInformation($"  {fileName}");

      return fileNames;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetFiles error");
      throw;
    }

  }

  private async Task DeleteImportFilesAsync(
    BlobContainerClient containerClient,
    string prefix,
    int? segmentSize)
  {
    try
    {
      var zipFile = $"{prefix}.zip";

      // Call the listing operation and return pages of the specified size.
      var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
          .AsPages(default, segmentSize);

      // Enumerate the blobs returned for each page.
      await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
      {
        // A hierarchical listing may return both virtual directories and blobs.
        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
        {
          if (blobhierarchyItem.IsPrefix)
          {
            // Call recursively with the prefix to traverse the virtual directory.
            await DeleteImportFilesAsync(containerClient, blobhierarchyItem.Prefix, null);
          }
          else
          {
            // don't delete the original import zip file
            if (zipFile != blobhierarchyItem.Blob.Name)
            {
              logger.LogInformation($" deleting existing: {blobhierarchyItem.Blob.Name}");
              await containerClient.DeleteBlobAsync(blobhierarchyItem.Blob.Name);
            }
          }
        }

        Console.WriteLine();
      }
    }
    catch (RequestFailedException e)
    {
      Console.WriteLine(e.Message);
      Console.ReadLine();
      throw;
    }
  }

  /// <summary>
  /// Calculate target directory for scoped type and id
  /// </summary>
  /// <param name="parentType">Scoped object type (e.g. 'Maps')</param>
  /// <param name="parentId">Scoped object id</param>
  /// <param name="fileName">Optional file name</param>
  /// <returns>Public directory for scope</returns>
  public override string GetPublicFileDirectory(string parentType, uint parentId, string fileName = "")
  {
    var targetDirectory = BuildPath(parentType, parentId.ToString());

    if (!string.IsNullOrEmpty(fileName))
      targetDirectory = $"{targetDirectory}{GetFolderSeparator()}{fileName}";

    return targetDirectory;
  }

  /// <summary>
  /// Gets the public URL for the file
  /// </summary>
  /// <param name="path"></param>
  /// <param name="fileName"></param>
  /// <returns></returns>
  public override string GetUrlPath(string path, string fileName)
  {
    var physicalPath = BuildPath(
      cfg.GetAppSettings().FileStorageUrl,
      path,
      fileName);

    return physicalPath;
  }
}