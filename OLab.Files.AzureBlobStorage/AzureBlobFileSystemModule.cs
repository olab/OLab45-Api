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
using static NuGet.Packaging.PackagingConstants;

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

    public char GetFolderSeparator() { return '/'; }

    private string GetScopedFileFolderName(string scopeLevel, uint scopeId)
    {
      var subPath = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{scopeLevel}{GetFolderSeparator()}{scopeId}";
      return subPath;
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
          var fileFolder = GetScopedFileFolderName(item.ImageableType, item.ImageableId);
          logger.LogInformation($"  file folder = '{fileFolder}");

          if (FileExists(fileFolder, item.Path))
          {
            var fileUrl = $"{_configuration.GetAppSettings().FileStorageUrl}{GetFolderSeparator()}{fileFolder}{GetFolderSeparator()}{item.Path}";
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

    /// <summary>
    /// Test if a file exists
    /// </summary>
    /// <param name="folderName">File folder</param>
    /// <param name="fileName">file name</param>
    /// <returns>true/false</returns>
    public bool FileExists(
      string folderName,
      string fileName)
    {
      var result = false;

      try
      {
        IList<BlobItem> blobs;

        var fullFileName = $"{folderName}{GetFolderSeparator()}{fileName}";

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
    /// <param name="sourceFolder">Source folder</param>
    /// <param name="destinationFolder">Destination folder</param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task MoveFileAsync(
      string fileName,
      string sourceFolder,
      string destinationFolder,
      CancellationToken token = default)
    {
      try
      {
        logger.LogInformation($"MoveFileAsync '{fileName}' {sourceFolder} -> {destinationFolder}");

        using (var sourceFileStream = new MemoryStream())
        {
          var sourceFilePath = $"{sourceFolder}{GetFolderSeparator()}{fileName}";
          await ReadFileAsync(sourceFileStream, sourceFolder, fileName, token);

          var destinationFilePath = $"{destinationFolder}{GetFolderSeparator()}{fileName}";
          await WriteFileAsync(sourceFileStream, destinationFolder, token);

          await DeleteFileAsync(sourceFolder, fileName);
        }

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "MoveFileAsync error");
        throw;
      }
    }

    public async Task<string> WriteFileAsync(
      Stream stream,
      string moduleFileName,
      CancellationToken token)
    {
      logger.LogInformation($"Write physical file: container {_containerName}, {moduleFileName}");

      await _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .UploadBlobAsync(moduleFileName, stream, token);

      return moduleFileName;
    }

    /// <summary>
    /// Read file from storage
    /// </summary>
    /// <param name="logger">OLabLogger</param>
    /// <param name="folderName">Source folder name</param>
    /// <param name="fileName">File name</param>
    /// <returns>File contents sourceFileStream</returns>
    public async Task ReadFileAsync(
      Stream stream,
      string folder,
      string fileName,
      CancellationToken token)
    {
      try
      {
        logger.LogInformation($"ReadFileAsync reading '{fileName}'");

        var sourceFolder = $"{_configuration.GetAppSettings().FileStorageFolder}{GetFolderSeparator()}{folder}";

        await _blobServiceClient
             .GetBlobContainerClient(_containerName)
             .GetBlobClient(sourceFolder)
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
    /// <param name="folderName">File folder</param>
    /// <param name="fileName">File to delete</param>
    /// <returns></returns>
    public async Task<bool> DeleteFileAsync(
      string folder,
      string fileName)
    {
      try
      {
        var sourceFolder = $"{folder}{GetFolderSeparator()}{fileName}";

        logger.LogInformation($"DeleteFileAsync '{sourceFolder}'");

        await _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .DeleteBlobAsync(sourceFolder);

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
      string extractDirectoryName,
      CancellationToken token)
    {
      try
      {
        logger.LogInformation($"extracting '{folderName}' {fileName} -> {extractDirectoryName}");

        using (var stream = new MemoryStream())
        {
          await ReadFileAsync(stream, folderName, fileName, token);

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