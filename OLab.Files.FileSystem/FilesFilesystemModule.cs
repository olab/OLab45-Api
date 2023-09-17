using Dawn;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Files.FileSystem
{
  [OLabModule("FILESYSTEM")]
  public class FilesFilesystemModule : IFileStorageModule
  {
    private readonly IOLabLogger _logger;
    private readonly IOLabConfiguration _configuration;

    public FilesFilesystemModule(
      IOLabLogger logger,
      IOLabConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
    }

    /// <summary>
    /// Attach URLs to access files
    /// </summary>
    /// <param name="items">SystemFiles records</param>
    public void AttachUrls(IList<SystemFiles> items)
    {
      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var subPath = GetBasePath(scopeLevel, scopeId, item.Path);
        var physicalPath = GetPhysicalPath(scopeLevel, scopeId, item.Path);

        if (FileExists(physicalPath))
          item.OriginUrl = $"/{Path.GetFileName(_configuration.GetAppSettings().Value.FileStorageFolder)}/{subPath}";
        else
          item.OriginUrl = null;
      }
    }

    /// <summary>
    /// Move file from one location to another
    /// </summary>
    /// <param name="sourcePath">Source path</param>
    /// <param name="destinationPath">Destination path</param>
    public void MoveFile(string sourcePath, string destinationPath)
    {
      Guard.Argument(sourcePath).NotEmpty(nameof(sourcePath));
      Guard.Argument(destinationPath).NotEmpty(nameof(destinationPath));

      File.Move(sourcePath, destinationPath);

      _logger.LogInformation($"moved file from '{sourcePath}' to {destinationPath}");
    }

    /// <summary>
    /// Test if file exists in storage
    /// </summary>
    /// <param name="physicalPath">Path to look for file</param>
    /// <returns>true/false</returns>
    public bool FileExists(string physicalPath)
    {
      return File.Exists(physicalPath);
    }

    /// <summary>
    /// Uploads a file to upload directory
    /// </summary>
    /// <param name="file">File contents stream</param>
    /// <param name="fileName">(Optional) file name (temp name generated, if null)</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Physical file path</returns>
    public async Task<string> UploadFile(
      Stream file,
      string fileName,
      CancellationToken token)
    {
      if (string.IsNullOrEmpty(fileName))
        fileName = Path.GetRandomFileName();

      var physicalPath = Path.Combine(_configuration.GetAppSettings().Value.ImportFolder, fileName);

      using (var stream = new FileStream(physicalPath, FileMode.Create))
      {
        await file.CopyToAsync(stream);
        _logger.LogInformation($"uploaded file to '{physicalPath}'. Size: {file.Length}");
      }

      return physicalPath;
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