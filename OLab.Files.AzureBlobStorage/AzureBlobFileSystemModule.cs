using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Configuration;
using OLab.Api.Utils;
using Microsoft.AspNetCore.Http.Metadata;
using static System.Formats.Asn1.AsnWriter;
using System.Net.NetworkInformation;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    private readonly Dictionary<string, IList<BlobItem>>
      _folderContentCache = new();

    public AzureBlobFileSystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _configuration = configuration;

      logger.LogInformation($"Initializing AzureBlobFileSystemModule");

      var connectionString = _configuration.GetAppSettings().FileStorageConnectionString;
      if (string.IsNullOrEmpty(connectionString))
        throw new ConfigurationErrorsException("missing FileStorageConnectionString parameter");

      _blobServiceClient = new BlobServiceClient(connectionString);
      _containerName = _configuration.GetAppSettings().FileStorageContainer;
      if (string.IsNullOrEmpty(_containerName))
        throw new ConfigurationErrorsException("missing FileStorageContainer parameter");

      logger.LogInformation($"FileStorageFolder: {_configuration.GetAppSettings().FileStorageFolder}");
      logger.LogInformation($"FileImportFolder: {_configuration.GetAppSettings().FileImportFolder}");
      logger.LogInformation($"FileStorageContainer: {_configuration.GetAppSettings().FileStorageContainer}");
      logger.LogInformation($"FileStorageUrl: {_configuration.GetAppSettings().FileStorageUrl}");
    }

    public void AttachUrls(IOLabLogger logger, IList<SystemFiles> items)
    {
      logger.LogInformation($"Attaching Azure Blob URLs for {items.Count} file records");

      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var baseFolder = GetFileFolderName(scopeLevel, scopeId);
        logger.LogInformation($"  folderName = '{baseFolder}");

        if (FileExists(logger, baseFolder, item.Path))
        {
          var fileUrl = $"{_configuration.GetAppSettings().FileStorageUrl}/{baseFolder}/{item.Path}";
          item.OriginUrl = fileUrl;
          logger.LogInformation($"  '{item.Path}' mapped to url '{item.OriginUrl}'");
        }
        else
          item.OriginUrl = null;

      }
    }

    private string GetFileFolderName(string scopeLevel, uint scopeId)
    {
      var subPath = $"{_configuration.GetAppSettings().FileStorageFolder}/{scopeLevel}/{scopeId}";
      return subPath;
    }

    public bool FileExists(IOLabLogger logger, string folderName, string physicalFileName)
    {
      bool result = false;

      try
      {
        IList<BlobItem> blobs;

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

        result = blobs.Any(x => x.Name.Contains(physicalFileName));

        if (!result)
          logger.LogWarning($"  '{folderName}/{physicalFileName}' physical file not found");

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "FileExists error");
        throw;
      }


      return result;
    }

    public void MoveFile(
      IOLabLogger logger,
      string sourcePath,
      string destinationPath)
    {
      throw new NotImplementedException();
    }

    public async Task<string> UploadFile(
      IOLabLogger logger,
      Stream file,
      string uploadedFileName,
      CancellationToken token)
    {
      try
      {
        var fileName = Path.GetRandomFileName();
        fileName += Path.GetExtension(uploadedFileName);

        logger.LogInformation($"Uploading file '{uploadedFileName}' ({fileName}) to '{_containerName}'");

        await _blobServiceClient
              .GetBlobContainerClient(_containerName)
              .UploadBlobAsync($"{_configuration.GetAppSettings().FileImportFolder}/{fileName}", file, token);

        return $"{fileName}";

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "UploadFile error");
        throw;
      }

    }

    private string GetPhysicalPath(string scopeLevel, uint scopeId, string fileName)
    {
      var subPath = GetFileFolderName(scopeLevel, scopeId);
      subPath += $"{subPath}/{fileName}";
      return subPath;
    }
  }
}