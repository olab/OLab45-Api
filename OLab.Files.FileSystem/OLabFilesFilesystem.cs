using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Data.Interface;

namespace OLab.Files.FileSystem
{
  [OLabModule("FILESYSTEM")]
  public class OLabFilesFilesystem : IFileStorageModule
  {
    private readonly OLabLogger _logger;

    public OLabFilesFilesystem(OLabLogger logger)
    {
      _logger = logger;
    }

    public void AttachUrls(AppSettings appSettings, IList<SystemFiles> items)
    {
      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeLevelId = item.ImageableId;

        var subPath = $"{scopeLevel}/{scopeLevelId}/{item.Path}";
        var physicalPath = Path.Combine(appSettings.FileStorageFolder, subPath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(physicalPath))
          item.OriginUrl = $"/{Path.GetFileName(appSettings.FileStorageFolder)}/{subPath}";
        else
          item.OriginUrl = null;
      }
    }
  }
}