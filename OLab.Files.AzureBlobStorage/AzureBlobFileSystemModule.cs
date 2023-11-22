using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dawn;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Configuration;
using System.IO;
using System.IO.Compression;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger logger;
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
      this.logger = logger;
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

    private string GetPhysicalPath(string folderName)
    {
      var physicalPath = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{folderName}";
      return physicalPath;
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
      logger.LogInformation($"Attaching Azure Blob URLs for {items.Count} sourceFileStream records");

      foreach (var item in items)
      {
        try
        {
          var scopeFolder = GetScopedFolderName(item.ImageableType, item.ImageableId);
          logger.LogInformation($"  file scope folder name = '{scopeFolder}");

          if (FileExists(scopeFolder, item.Path))
          {
            item.OriginUrl = $"{_configuration.GetAppSettings().FileStorageUrl}{GetFolderSeparator()}{Path.GetFileName(_configuration.GetAppSettings().FileStorageFolder)}{GetFolderSeparator()}{scopeFolder}{GetFolderSeparator()}{item.Path}";
            logger.LogInformation($"  file {item.Name}({item.Id}): '{item.Path}' mapped to url '{item.OriginUrl}'");
          }
          else
            item.OriginUrl = null;

        }
        catch (Exception ex)
        {
          logger.LogError(ex, $"AttachUrls error on '{item.Path}'");
        }

      }
    }

    /// <summary>
    /// Test if a file exists
    /// </summary>
    /// <param name="folderName">File folderName</param>
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

        var physicalPath = GetPhysicalPath(folderName);

        // if we do not have this folderName already in cache
        // then hit the blob storage and cache the results
        if (!_folderContentCache.ContainsKey(physicalPath))
        {
          logger.LogInformation($"reading '{folderName}' for files");

          blobs = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobs(prefix: physicalPath).ToList();

          logger.LogInformation($"found '{blobs.Count}' file blobs");

          foreach( var blob in blobs)
            logger.LogInformation($"  found '{blob.Name}'");

          _folderContentCache[physicalPath] = blobs;
        }
        else
          blobs = _folderContentCache[physicalPath];

        result = blobs.Any(x => x.Name.Contains(fileName));

        if (!result)
          logger.LogWarning($"  '{folderName}{GetFolderSeparator()}{fileName}' physical sourceFileStream not found");
        else
          logger.LogInformation($"  '{folderName}{GetFolderSeparator()}{fileName}' found");

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "FileExists error");
        throw;
      }


      return result;
    }

    /// <summary>
    /// Move file 
    /// </summary>
    /// <param name="logger">OLabLogger </param>
    /// <param name="fileName">File name</param>
    /// <param name="sourceFolder">Source folderName</param>
    /// <param name="destinationFolder">Destination folderName</param>
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
        logger.LogInformation($"MoveFileAsync '{fileName}' {sourceFolder} -> {destinationFolder}");

        using (var sourceFileStream = new MemoryStream())
        {
          var sourceFilePath = $"{sourceFolder}{GetFolderSeparator()}{fileName}";
          await CopyStreamToFileAsync(sourceFileStream, sourceFolder, fileName, token);

          var destinationFilePath = $"{destinationFolder}{GetFolderSeparator()}{fileName}";
          await CopyFiletoStreamAsync(sourceFileStream, destinationFolder, token);

          await DeleteFileAsync(sourceFolder, fileName);
        }

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
    /// <param name="folderName">Target folder</param>
    /// <param name="fileName">Target file name</param>
    /// <param name="token"></param>
    public async Task<string> CopyFiletoStreamAsync(
      Stream stream,
      string targetFolder,
      CancellationToken token)
    {
      Guard.Argument(stream).NotNull(nameof(stream));
      Guard.Argument(targetFolder).NotEmpty(nameof(targetFolder));

      try
      {
        logger.LogInformation($"Write physical file: container {_containerName}, {targetFolder}");

        await _blobServiceClient
              .GetBlobContainerClient(_containerName)
              .UploadBlobAsync(targetFolder, stream, token);

        return targetFolder;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "CopyFiletoStreamAsync Exception");
        throw;
      }

    }

    /// <summary>
    /// Read file from storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">Source folderName name</param>
    /// <param name="fileName">File name</param>
    /// <returns>File contents sourceFileStream</returns>
    public async Task CopyStreamToFileAsync(
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
        logger.LogInformation($"CopyStreamToFileAsync reading '{fileName}'");

        var physicalFileName = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{folderName}{GetFolderSeparator()}{fileName}";

        await _blobServiceClient
             .GetBlobContainerClient(_containerName)
             .GetBlobClient(physicalFileName)
             .DownloadToAsync(stream);

        stream.Position = 0;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "CopyStreamToFileAsync Exception");
        throw;
      }

    }

    /// <summary>
    /// Delete file from blob storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">File folderName</param>
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
        var physicalFileName = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{folderName}{GetFolderSeparator()}{fileName}";

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
    /// Extract a file to blob storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="archiveFileName">Source file name</param>
    /// <param name="extractDirectory">TTarget extreaction folderName</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async Task<bool> ExtractFileToStorageAsync(
      string folderName,
      string fileName,
      string extractDirectoryName,
      CancellationToken token)
    {
      Guard.Argument(folderName).NotEmpty(nameof(folderName));
      Guard.Argument(fileName).NotEmpty(nameof(fileName));
      Guard.Argument(extractDirectoryName).NotEmpty(nameof(extractDirectoryName));

      try
      {
        logger.LogInformation($"extracting '{folderName}' {fileName} -> {extractDirectoryName}");

        using (var stream = new MemoryStream())
        {
          await CopyStreamToFileAsync(stream, folderName, fileName, token);

          var fileProcessor = new ZipFileProcessor(
            _blobServiceClient.GetBlobContainerClient(_containerName),
            logger,
            _configuration);

          var extractPath = $"{_configuration.GetAppSettings().FileImportFolder}{GetFolderSeparator()}{folderName}{GetFolderSeparator()}{fileName}.import";
          await fileProcessor.ProcessFileAsync(stream, extractPath, token);

        }

        return true;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "ExtractFileToStorageAsync Exception");
        throw;
      }

    }

    /// <summary>
    /// Create archvie file from a folder
    /// </summary>
    /// <param name="archive">Archive file stream</param>
    /// <param name="folderName">Source file folder</param>
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

          var entryPath = $"{folderName}{GetFolderSeparator()}{Path.GetFileName(blob.Name)}";
          logger.LogInformation($"  adding '{blob.Name}' to archive '{entryPath}'. size = {blobStream.Length}");

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
        logger.LogError(ex, "CopyFoldertoArchiveAsync error");
        throw;
      }


      return result;
    }
  }
}