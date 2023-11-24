using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dawn;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Configuration;
using System.IO.Compression;
using System.Text;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger _logger;
    private readonly IOLabConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    private readonly Dictionary<string, IList<BlobItem>>
      _folderContentCache = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">OlabLogger</param>
    /// <param name="configuration">Application configuration</param>
    /// <exception cref="ConfigurationErrorsException"></exception>
    public AzureBlobFileSystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;

      logger.LogInformation($"Initializing AzureBlobFileSystemModule");

      // if not set to use this module, then don't proceed further
      if (GetModuleName().ToLower() != _configuration.GetAppSettings().FileStorageType.ToLower())
        return;

      var connectionString = _configuration.GetAppSettings().FileStorageConnectionString;
      if (string.IsNullOrEmpty(connectionString))
        throw new ConfigurationErrorsException("missing FileStorageConnectionString parameter");

      _blobServiceClient = new BlobServiceClient(connectionString);
      _containerName = _configuration.GetAppSettings().FileStorageContainer;
      if (string.IsNullOrEmpty(_containerName))
        throw new ConfigurationErrorsException("missing FileStorageContainer parameter");

      logger.LogInformation($"FileStorageFolder: {_configuration.GetAppSettings().FileStorageFolder}");
      logger.LogInformation($"FileStorageContainer: {_configuration.GetAppSettings().FileStorageContainer}");
      logger.LogInformation($"FileStorageUrl: {_configuration.GetAppSettings().FileStorageUrl}");
    }

    public string GetModuleName()
    {
      var attrib = this.GetType().GetCustomAttributes(typeof(OLabModuleAttribute), true).FirstOrDefault() as OLabModuleAttribute;
      return attrib == null ? "" : attrib.Name;
    }

    public char GetFolderSeparator() { return '/'; }

    private string GetScopedFolderName(string scopeLevel, uint scopeId)
    {
      var scopeFolder = $"{scopeLevel}{GetFolderSeparator()}{scopeId}";
      return scopeFolder;
    }

    /// <summary>
    /// Attach browseable URLS for system files
    /// </summary>
    /// <param name="items">List of system files objects to enhance</param>
    public void AttachUrls(IList<SystemFiles> items)
    {
      _logger.LogInformation($"Attaching Azure Blob URLs for {items.Count} sourceFileStream records");

      foreach (var item in items)
      {
        try
        {
          var scopeFolder = BuildPath(
            _configuration.GetAppSettings().FileStorageContainer,
            GetScopedFolderName(item.ImageableType, item.ImageableId)
          );

          _logger.LogInformation($"  file scope folderName name = '{scopeFolder}");

          if (FileExists(scopeFolder, item.Path))
          {
            item.OriginUrl = $"{_configuration.GetAppSettings().FileStorageUrl}{GetFolderSeparator()}{Path.GetFileName(_configuration.GetAppSettings().FileStorageFolder)}{GetFolderSeparator()}{scopeFolder}{GetFolderSeparator()}{item.Path}";
            _logger.LogInformation($"  file {item.Name}({item.Id}): '{item.Path}' mapped to url '{item.OriginUrl}'");
          }
          else
          {
            _logger.LogWarning($"  '{scopeFolder}/{item.Path}' not found");
            item.OriginUrl = null;
          }

        }
        catch (Exception ex)
        {
          _logger.LogError(ex, $"AttachUrls error on '{item.Path}'");
        }

      }
    }

    /// <summary>
    /// Test if a file exists
    /// </summary>
    /// <param name="folderName">File sourceFolderName</param>
    /// <param name="fileName">file name</param>
    /// <returns>true/false</returns>
    public bool FileExists(
      string folderName,
      string fileName)
    {
      Guard.Argument(folderName).NotEmpty(nameof(folderName));
      Guard.Argument(fileName).NotEmpty(nameof(fileName));

      var result = false;

      try
      {
        IList<BlobItem> blobs;

        _logger.LogInformation($"looking for existance of '{folderName}/{fileName}'");

        // if we do not have this sourceFolderName already in cache
        // then hit the blob storage and cache the results
        if (!_folderContentCache.ContainsKey(folderName))
        {
          blobs = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobs(prefix: folderName).ToList();

          _folderContentCache[folderName] = blobs;
        }
        else
          blobs = _folderContentCache[folderName];

        result = blobs.Any(x => x.Name.Contains(fileName));

        if (!result)
          _logger.LogWarning($"  '{folderName}' not found");
        else
          _logger.LogInformation($"  '{folderName}' found");

      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "FileExists error");
        throw;
      }


      return result;
    }

    /// <summary>
    /// Move file 
    /// </summary>
    /// <param name="logger">OLabLogger </param>
    /// <param name="fileName">File name</param>
    /// <param name="sourceFolder">Source sourceFolderName</param>
    /// <param name="destinationFolder">Destination sourceFolderName</param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task MoveFileAsync(
      string fileName,
      string sourceFolder,
      string destinationFolder,
      CancellationToken token = default)
    {
      Guard.Argument(sourceFolder).NotEmpty(nameof(sourceFolder));
      Guard.Argument(destinationFolder).NotEmpty(nameof(destinationFolder));

      try
      {
        _logger.LogInformation($"MoveFileAsync '{fileName}' {sourceFolder} -> {destinationFolder}");

        using (var sourceFileStream = new MemoryStream())
        {
          await ReadFileAsync(sourceFileStream, sourceFolder, fileName, token);
          await WriteFileAsync(sourceFileStream, destinationFolder, fileName, token);
          await DeleteFileAsync(sourceFolder, fileName);
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "MoveFileAsync error");
        throw;
      }
    }

    /// <summary>
    /// Copy file presented by stream to file store
    /// </summary>
    /// <param name="stream">File stream</param>
    /// <param name="folderName">Target folderName</param>
    /// <param name="token"></param>
    public async Task<string> WriteFileAsync(
      Stream stream,
      string folderName,
      string fileName,
      CancellationToken token)
    {
      Guard.Argument(stream).NotNull(nameof(stream));
      Guard.Argument(folderName).NotEmpty(nameof(folderName));
      Guard.Argument(fileName).NotEmpty(nameof(fileName));

      try
      {
        var physicalFilename = BuildPath(folderName, fileName);
        _logger.LogInformation($"WriteFileAsync: {_containerName} {physicalFilename}");

        await _blobServiceClient
              .GetBlobContainerClient(_containerName)
              .GetBlobClient(physicalFilename)
              .UploadAsync(stream, overwrite: true, token);

        return folderName;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "WriteFileAsync Exception");
        throw;
      }

    }

    /// <summary>
    /// ReadAsync file from storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">Source sourceFolderName name</param>
    /// <param name="fileName">File name</param>
    /// <returns>File contents sourceFileStream</returns>
    public async Task ReadFileAsync(
      Stream stream,
      string folderName,
      string fileName,
      CancellationToken token)
    {
      Guard.Argument(stream).NotNull(nameof(stream));
      Guard.Argument(folderName).NotEmpty(nameof(folderName));
      Guard.Argument(fileName).NotEmpty(nameof(fileName));

      try
      {

        var physicalFileName = $"{folderName}{GetFolderSeparator()}{fileName}";
        _logger.LogInformation($"ReadFileAsync: {_containerName} {physicalFileName}");

        await _blobServiceClient
             .GetBlobContainerClient(_containerName)
             .GetBlobClient(physicalFileName)
             .DownloadToAsync(stream);

        stream.Position = 0;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ReadFileAsync Exception");
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
    public async Task<bool> DeleteFileAsync(
      string folderName,
      string fileName)
    {
      Guard.Argument(folderName).NotEmpty(nameof(folderName));
      Guard.Argument(fileName).NotEmpty(nameof(fileName));

      try
      {
        var physicalFileName = $"{folderName}{GetFolderSeparator()}{fileName}";

        _logger.LogInformation($"DeleteFileAsync '{physicalFileName}'");

        await _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .DeleteBlobAsync(physicalFileName);

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "DeleteFileAsync Exception");
        throw;
      }

    }

    public async Task DeleteImportFilesAsync(
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
                _logger.LogInformation($" deleting existing: {blobhierarchyItem.Blob.Name}");
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
    /// Delete folder from blob storage
    /// </summary>
    /// <param name="folderName">Folder to delete</param>
    public async Task DeleteFolderAsync(string folderName)
    {
      await DeleteImportFilesAsync(
        _blobServiceClient.GetBlobContainerClient(_containerName),
        folderName,
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
    public async Task<bool> ExtractFileToStorageAsync(
      string sourceFolderName,
      string sourceFileName,
      string targetDirectoryName,
      CancellationToken token)
    {
      Guard.Argument(sourceFolderName).NotEmpty(nameof(sourceFolderName));
      Guard.Argument(sourceFileName).NotEmpty(nameof(sourceFileName));
      Guard.Argument(targetDirectoryName).NotEmpty(nameof(targetDirectoryName));

      try
      {
        _logger.LogInformation($"extracting '{sourceFolderName}' {sourceFileName} -> {targetDirectoryName}");

        using (var stream = new MemoryStream())
        {
          var fileProcessor = new ZipFileProcessor(
            _blobServiceClient.GetBlobContainerClient(_containerName),
            _logger,
            _configuration);

          var extractPath = $"{sourceFolderName}{GetFolderSeparator()}{Path.GetFileNameWithoutExtension(sourceFileName)}";

          await ReadFileAsync(stream, sourceFolderName, sourceFileName, token);
          await fileProcessor.ProcessFileAsync(stream, extractPath, token);
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ExtractFileToStorageAsync Exception");
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
    public async Task<bool> CopyFoldertoArchiveAsync(
      ZipArchive archive,
      string folderName,
      bool appendToStream,
      CancellationToken token)
    {
      Guard.Argument(archive).NotNull(nameof(archive));
      Guard.Argument(folderName).NotEmpty(nameof(folderName));

      var result = false;

      try
      {
        IList<BlobItem> blobs;

        var physicalFolder = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{folderName}";
        _logger.LogInformation($"reading '{physicalFolder}' for files to add to stream");

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

          var entryPath = $"{folderName}{GetFolderSeparator()}{Path.GetFileName(blob.Name)}";
          _logger.LogInformation($"  adding '{blob.Name}' to archive '{entryPath}'. size = {blobStream.Length}");

          var entry = archive.CreateEntry(entryPath);
          using (var entryStream = entry.Open())
          {
            blobStream.CopyTo(entryStream);
            entryStream.Close();
          }

        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "CopyFoldertoArchiveAsync error");
        throw;
      }


      return result;
    }

    public IList<string> GetFiles(string folderName, CancellationToken token)
    {
      var fileNames = new List<string>();

      try
      {
        _logger.LogInformation($"reading '{folderName}' for files");

        var blobs = _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .GetBlobs(prefix: folderName).ToList();

        if (blobs.Count == 0)
          return fileNames;

        _logger.LogInformation($"  found '{blobs.Count}' file blobs");
        fileNames = blobs.Select(blob => Path.GetFileName(blob.Name)).ToList();

        foreach (var fileName in fileNames)
          _logger.LogInformation($"  {fileName}");

        return fileNames;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "GetFiles error");
        throw;
      }

    }

    /// <summary>
    /// Builds a path, compatible with the file module
    /// </summary>
    /// <param name="pathParts">Argument list of path parts</param>
    /// <returns>Path string</returns>
    public string BuildPath(params object[] pathParts)
    {
      var sb = new StringBuilder();
      for (int i = 0; i < pathParts.Length; i++)
      {
        sb.Append(pathParts[i].ToString());
        if (i < pathParts.Length - 1)
          sb.Append(GetFolderSeparator());
      }

      return sb.ToString();
    }

  }
}