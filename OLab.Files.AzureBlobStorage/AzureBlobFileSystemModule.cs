using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Configuration;
using System.Reflection;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger logger;
    private readonly IOLabConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    private readonly string _importBaseFolder;

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

      _importBaseFolder = $"{_configuration.GetAppSettings().FileImportFolder}{GetFolderSeparator()}";

      logger.LogInformation($"FileStorageFolder: {_configuration.GetAppSettings().FileStorageFolder}");
      logger.LogInformation($"FileImportFolder: {_importBaseFolder}");
      logger.LogInformation($"FileStorageContainer: {_configuration.GetAppSettings().FileStorageContainer}");
      logger.LogInformation($"FileStorageUrl: {_configuration.GetAppSettings().FileStorageUrl}");
    }

    public char GetFolderSeparator() { return '/'; }

    /// <summary>
    /// Attach browseable URLS for system files
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="items">List of system files to enhance</param>
    public void AttachUrls(IList<SystemFiles> items)
    {
      logger.LogInformation($"Attaching Azure Blob URLs for {items.Count} stream records");

      foreach (var item in items)
      {
        try
        {
          var scopeLevel = item.ImageableType;
          var scopeId = item.ImageableId;

          var baseFolder = GetFileFolderName(scopeLevel, scopeId);
          logger.LogInformation($"  files baseFolder = '{baseFolder}");

          if (FileExists(baseFolder, item.Path))
          {
            var fileUrl = $"{_configuration.GetAppSettings().FileStorageUrl}{GetFolderSeparator()}{baseFolder}{GetFolderSeparator()}{item.Path}";
            item.OriginUrl = fileUrl;
            logger.LogInformation($"  '{item.Path}' mapped to url '{item.OriginUrl}'");
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

    private string GetFileFolderName(string scopeLevel, uint scopeId)
    {
      var subPath = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{scopeLevel}{GetFolderSeparator()}{scopeId}";
      return subPath;
    }

    /// <summary>
    /// Test if a file exists
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">File folder</param>
    /// <param name="physicalFileName">file name</param>
    /// <returns>true/false</returns>
    public bool FileExists(
      string folderName,
      string physicalFileName)
    {
      var result = false;

      try
      {
        IList<BlobItem> blobs;

        var fullFileName = $"{folderName}{GetFolderSeparator()}{physicalFileName}";

        // if we do not have this folder already in cache
        // then hit the blob storage and cache the results
        if (!_folderContentCache.ContainsKey(folderName))
        {
          logger.LogInformation($"reading '{folderName}' for files");

          blobs = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobs(prefix: folderName).ToList();

          _folderContentCache[folderName] = blobs;
        }
        else
          blobs = _folderContentCache[folderName];

        result = blobs.Any(x => x.Name.Contains(fullFileName));

        if (!result)
          logger.LogWarning($"  '{folderName}{GetFolderSeparator()}{physicalFileName}' physical stream not found");
        else
          logger.LogInformation($"  '{folderName}{GetFolderSeparator()}{physicalFileName}' found");

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
    /// <param name="sourcePath">Source folder</param>
    /// <param name="destinationPath">Destination folder</param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task MoveFileAsync(
      string fileName,
      string sourcePath,
      string destinationPath,
      CancellationToken token = default)
    {
      try
      {
        logger.LogInformation($"MoveFileAsync '{fileName}' {sourcePath} -> {destinationPath}");

        var stream = await ReadFileAsync(sourcePath, fileName);

        var destinationfilePath = $"{destinationPath}{GetFolderSeparator()}{fileName}";
        await WriteFileAsync(stream, destinationfilePath, token);
        await DeleteFileAsync(sourcePath, fileName);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "MoveFileAsync error");
        throw;
      }
    }

    /// <summary>
    /// Upload an import file to storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="stream">File contents stream</param>
    /// <param name="uploadedFileName">Source file name</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Fully qualified file name</returns>
    public async Task<string> UploadImportFileAsync(
      Stream stream,
      string uploadedFileName,
      CancellationToken token)
    {
      try
      {
        // build an offuscated random.zip file name to upload
        // import file to, with target import folder
        var tempFileName = Path.GetRandomFileName().Replace(".", "");
        var fileName = $"{tempFileName}{Path.GetExtension(uploadedFileName)}";
        var filePath = $"{_importBaseFolder}{fileName}";

        logger.LogInformation($"UploadFileAsync '{uploadedFileName}' ({filePath}) to '{_containerName}'");

        await WriteFileAsync(stream, filePath, token);

        return fileName;

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "UploadFileAsync Exception");
        throw;
      }

    }

    /// <summary>
    /// Upload a map file to storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="stream">File contents stream</param>
    /// <param name="uploadedFileName">Source file name</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="mapId"></param>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<string> UploadMapFileAsync(
      Stream stream, 
      FilesFullDto dto, 
      CancellationToken token)
    {
      throw new NotImplementedException();
    }

    private async Task WriteFileAsync(
      Stream stream,
      string filePath,
      CancellationToken token)
    {
      await _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .UploadBlobAsync(filePath, stream, token);
    }

    /// <summary>
    /// Read file from storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">Source folder name</param>
    /// <param name="fileName">File name</param>
    /// <returns>File contents stream</returns>
    public async Task<Stream> ReadFileAsync(
      string folderName,
      string fileName)
    {
      var stream = new MemoryStream();

      try
      {
        var filePath = string.IsNullOrEmpty(folderName) ?
          $"{_importBaseFolder}{fileName}" :
          $"{_importBaseFolder}{folderName}{GetFolderSeparator()}{fileName}";

        logger.LogInformation($"ReadFileAsync reading '{filePath}'");

        await _blobServiceClient
             .GetBlobContainerClient(_containerName)
             .GetBlobClient(filePath)
             .DownloadToAsync(stream);

        stream.Position = 0;
        return stream;

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
    /// <param name="folderName">File folder</param>
    /// <param name="fileName">File to delete</param>
    /// <returns></returns>
    public async Task<bool> DeleteFileAsync(
      string folderName,
      string fileName)
    {
      try
      {
        var filePath = string.IsNullOrEmpty(folderName) ?
          $"{_importBaseFolder}{fileName}" :
          $"{_importBaseFolder}{folderName}{GetFolderSeparator()}{fileName}";

        logger.LogInformation($"DeleteFileAsync '{filePath}'");

        await _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .DeleteBlobAsync(filePath);

        return true;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "DeleteFileAsync Exception");
      }

      return false;

    }

    /// <summary>
    /// Extract a file to blob storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="archiveFileName">Source file name</param>
    /// <param name="extractDirectory">TTarget extreaction folder</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async Task<bool> ExtractFileAsync(
      string folderName,
      string fileName,
      string extractDirectory,
      CancellationToken token)
    {
      try
      {
        logger.LogInformation($"ExtractFileAsync '{folderName}' {fileName} -> {extractDirectory}");

        var stream = await ReadFileAsync(folderName, fileName);

        var fileProcessor = new ZipFileProcessor(
          _blobServiceClient.GetBlobContainerClient(_containerName),
          logger,
          _configuration);

        var extractPath = string.IsNullOrEmpty(folderName) ?
          $"{_importBaseFolder}{extractDirectory}" :
          $"{_importBaseFolder}{folderName}{GetFolderSeparator()}{extractDirectory}";

        await fileProcessor.ProcessFileAsync($"{_importBaseFolder}{fileName}", extractPath, stream, token);

        return true;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "ExtractFileAsync Exception");
        throw;
      }

    }

    public string GetModuleName()
    {
      var attrib = this.GetType().GetCustomAttributes(typeof(OLabModuleAttribute), true).FirstOrDefault() as OLabModuleAttribute;
      return attrib == null ? "" : attrib.Name;
    }

  }
}