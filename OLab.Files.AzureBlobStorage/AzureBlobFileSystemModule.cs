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

    public AzureBlobFileSystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _logger = OLabLogger.CreateNew<AzureBlobFileSystemModule>(logger);
      _configuration = configuration;

      _logger.LogInformation($"Initializing AzureBlobFileSystemModule");

      var connectionString = _configuration.GetAppSettings().FileStorageConnectionString;
      if (string.IsNullOrEmpty(connectionString))
        throw new ConfigurationErrorsException("missing FileStorageConnectionString parameter");

      _blobServiceClient = new BlobServiceClient(connectionString);
      _containerName = _configuration.GetAppSettings().FileStorageContainer;
      if (string.IsNullOrEmpty(_containerName))
        throw new ConfigurationErrorsException("missing FileStorageContainer parameter");
    }

    public void AttachUrls(IList<SystemFiles> items)
    {
      _logger.LogInformation($"Attaching Azure Blob URLs for {items.Count} file records");

      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var baseFolder = GetBasePath(scopeLevel, scopeId);

        if (FileExists(baseFolder, item.Path))
        {
          var fileUrl = $"{_configuration.GetAppSettings().FileStorageUrl}/{baseFolder}/{item.Path}";
          item.OriginUrl = fileUrl;
          _logger.LogInformation($"  '{item.Path}' mapped to url '{item.OriginUrl}'");
        }
        else
          item.OriginUrl = null;

      }
    }

    public bool FileExists(string baseFolder, string physicalFileName)
    {
      IList<BlobItem> blobs;

      // if we do not have this folder already in cache
      // then hit the blob storage and cache the results
      if (!_folderContentCache.ContainsKey(baseFolder))
      {
        _logger.LogInformation($"reading '{baseFolder}' for files");

        blobs = _blobServiceClient
          .GetBlobContainerClient(_containerName)
          .GetBlobs(prefix: baseFolder).ToList();
        _folderContentCache[baseFolder] = blobs;
      }
      else
        blobs = _folderContentCache[baseFolder];

      var result = blobs.Any(x => x.Name.Contains(physicalFileName));

      if (!result)
        _logger.LogWarning($"  '{baseFolder}/{physicalFileName}' physical file not found");

      return result;
    }

    public void MoveFile(
      string sourcePath,
      string destinationPath)
    {
      throw new NotImplementedException();
    }

    public Task<string> UploadFile(
      Stream file,
      string fileName,
      CancellationToken token)
    {
      throw new NotImplementedException();
    }

    private string GetBasePath(string scopeLevel, uint scopeId)
    {
      var subPath = $"{_configuration.GetAppSettings().FileStorageFolder}/{scopeLevel}/{scopeId}";
      return subPath;
    }

    private string GetPhysicalPath(string scopeLevel, uint scopeId, string fileName)
    {
      var subPath = GetBasePath(scopeLevel, scopeId);
      subPath += $"{subPath}/{fileName}";
      return subPath;
    }
  }
}