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

        var baseFolder = GetBasePath(scopeLevel, scopeId);
        logger.LogInformation($"  baseFolder = '{baseFolder}");

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

    private string GetBasePath(string scopeLevel, uint scopeId)
    {
      var subPath = $"{_configuration.GetAppSettings().FileStorageFolder}/{scopeLevel}/{scopeId}";
      return subPath;
    }

    public bool FileExists(IOLabLogger logger, string baseFolder, string physicalFileName)
    {
      bool result = false;

      try
      {
        IList<BlobItem> blobs;

        // if we do not have this folder already in cache
        // then hit the blob storage and cache the results
        if (!_folderContentCache.ContainsKey(baseFolder))
        {
          logger.LogInformation($"reading '{baseFolder}' for files");

          blobs = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobs(prefix: baseFolder).ToList();
          _folderContentCache[baseFolder] = blobs;
        }
        else
          blobs = _folderContentCache[baseFolder];

        result = blobs.Any(x => x.Name.Contains(physicalFileName));

        if (!result)
          logger.LogWarning($"  '{baseFolder}/{physicalFileName}' physical file not found");

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

    public Task<string> UploadFile(
      IOLabLogger logger,
      Stream file,
      string fileName,
      CancellationToken token)
    {
      throw new NotImplementedException();
    }

    private string GetPhysicalPath(string scopeLevel, uint scopeId, string fileName)
    {
      var subPath = GetBasePath(scopeLevel, scopeId);
      subPath += $"{subPath}/{fileName}";
      return subPath;
    }
  }
}