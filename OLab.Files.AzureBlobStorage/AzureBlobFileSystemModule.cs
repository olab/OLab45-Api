using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger _logger;
    private readonly IOLabConfiguration _configuration;

    public AzureBlobFileSystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
    }

    public void AttachUrls(IList<SystemFiles> items)
    {
      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var subPath = GetBasePath(scopeLevel, scopeId, item.Path);
        var physicalPath = GetPhysicalPath(scopeLevel, scopeId, item.Path);

        if (FileExists(physicalPath))
          item.OriginUrl = $"/{Path.GetFileName(_configuration.GetAppSettings().Value.FileStorageUrl)}/{subPath}";
        else
          item.OriginUrl = null;
      }
    }

    public bool FileExists(string physicalPath)
    {
      throw new NotImplementedException();
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

    private string GetBasePath(string scopeLevel, uint scopeId, string filePath)
    {
      var subPath = $"{scopeLevel}/{scopeId}/{filePath}";
      return subPath;
    }

    private string GetPhysicalPath(string scopeLevel, uint scopeId, string filePath)
    {
      var subPath = GetBasePath(scopeLevel, scopeId, filePath);

      var physicalPath = Path.Combine(
        _configuration.GetAppSettings().Value.FileStorageFolder,
        subPath.Replace('/', Path.DirectorySeparatorChar));
      return physicalPath;
    }
  }
}