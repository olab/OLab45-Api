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
  /// <param name="folderName">File folder name</param>
  /// <param name="fileName">file name</param>
  /// <returns>true/false</returns>
  public override bool FileExists(
    string folderName,
    string fileName)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    var result = false;

    try
    {
      IList<BlobItem> blobs;

      var physicalPath = BuildPath(cfg.GetAppSettings().FileStorageRoot, folderName);

      // if we do not have this sourceFolderName already in cache
      // then hit the blob storage and cache the results
      if (!_folderContentCache.ContainsKey(physicalPath))
      {
        logger.LogInformation($"  searching '{physicalPath} for blobs'");

        blobs = _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .GetBlobs(prefix: physicalPath).ToList();

        _folderContentCache[physicalPath] = blobs;

        foreach (var blob in blobs)
          logger.LogInformation($"  found blob '{blob.Name}'");

      }
      else
        blobs = _folderContentCache[physicalPath];

      result = blobs.Any(x => x.Name.Contains(fileName));

      if (!result)
        logger.LogWarning($"  '{folderName}{GetFolderSeparator()}{fileName}' not found");
      else
        logger.LogInformation($"  '{folderName}{GetFolderSeparator()}{fileName}' exists");

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
  /// <param name="fileName">File name</param>
  /// <param name="sourceFolder">Source folder name</param>
  /// <param name="destinationFolder">Destination folder name</param>
  public override async Task MoveFileAsync(
    string fileName,
    string sourceFolder,
    string destinationFolder,
    CancellationToken token = default)
  {
    Guard.Argument(sourceFolder).NotEmpty(nameof(sourceFolder));
    Guard.Argument(destinationFolder).NotEmpty(nameof(destinationFolder));

    try
    {
      logger.LogInformation($"MoveFileAsync '{fileName}' {sourceFolder} -> {destinationFolder}");

      using var stream = new MemoryStream();
      await ReadFileAsync(stream, sourceFolder, fileName, token);
      await WriteFileAsync(stream, destinationFolder, fileName, token);
      await DeleteFileAsync(sourceFolder, fileName);

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
  /// <param name="folderName">Target folderName</param>
  /// <param name="fileName">Target folderName</param>
  /// <param name="token"></param>
  public override async Task<string> WriteFileAsync(
    Stream stream,
    string folderName,
    string fileName,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalFilename = GetPhysicalPath(folderName, fileName);
      logger.LogInformation($"WriteFileAsync: {_containerName} {physicalFilename}");

      await _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(physicalFilename)
            .UploadAsync(stream, overwrite: true, token);

      return folderName;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "WriteFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// ReadAsync file from storage
  /// </summary>
  /// <param name="logger">OLabLogger</param>
  /// <param name="folderName">Source sourceFolderName name</param>
  /// <param name="fileName">File name</param>
  /// <returns>File contents stream</returns>
  public override async Task ReadFileAsync(
    Stream stream,
    string folderName,
    string fileName,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {

      var physicalFileName = GetPhysicalPath(folderName, fileName);
      logger.LogInformation($"ReadFileAsync: {_containerName} {physicalFileName}");

      await _blobServiceClient
           .GetBlobContainerClient(_containerName)
           .GetBlobClient(physicalFileName)
           .DownloadToAsync(stream);

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
  /// <param name="folderName">File sourceFolderName</param>
  /// <param name="fileName">File to delete</param>
  /// <returns></returns>
  public override async Task<bool> DeleteFileAsync(
    string folderName,
    string fileName)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalFileName = GetPhysicalPath(folderName, fileName);
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
  public override async Task DeleteFolderAsync(string folderName)
  {
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
    string sourceFolderName,
    string sourceFileName,
    string targetDirectoryName,
    CancellationToken token = default)
  {
    Guard.Argument(sourceFolderName).NotEmpty(nameof(sourceFolderName));
    Guard.Argument(sourceFileName).NotEmpty(nameof(sourceFileName));
    Guard.Argument(targetDirectoryName).NotEmpty(nameof(targetDirectoryName));

    try
    {
      logger.LogInformation($"extracting '{sourceFolderName}' {sourceFileName} -> {targetDirectoryName}");

      using var stream = new MemoryStream();
      var fileProcessor = new ZipFileProcessor(
        _blobServiceClient.GetBlobContainerClient(_containerName),
        logger,
        cfg);

      await ReadFileAsync(
        stream,
        sourceFolderName,
        sourceFileName,
        token);

      var extractPath = GetPhysicalPath(
        sourceFolderName,
        Path.GetFileNameWithoutExtension(sourceFileName));

      await fileProcessor.ProcessFileAsync(
        stream,
        extractPath,
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

      var physicalFolderName = GetPhysicalPath(folderName);

      var blobs = _blobServiceClient
        .GetBlobContainerClient(_containerName)
        .GetBlobs(prefix: physicalFolderName).ToList();

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


}