using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Configuration;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger _logger;
    private readonly IOLabConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobFileSystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;

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
      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var baseFolder = GetBasePath(scopeLevel, scopeId);

        if (FileExists(baseFolder, item.Path))
          item.OriginUrl = $"/{Path.GetFileName(_configuration.GetAppSettings().FileStorageUrl)}/{baseFolder}";
        else
          item.OriginUrl = null;
      }
    }

    public bool FileExists(string baseFolder, string physicalFileName)
    {
      var blobs = _blobServiceClient
        .GetBlobContainerClient(_containerName)
        .GetBlobs(prefix: baseFolder).ToList();

      return blobs.Any(x => x.Name == physicalFileName);
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
      var subPath = $"{scopeLevel}/{scopeId}";
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