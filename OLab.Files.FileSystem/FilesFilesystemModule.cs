using Dawn;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Files.FileSystem;

#pragma warning disable CS1998

[OLabModule("FILESYSTEM")]
public class FilesFilesystemModule : IFileStorageModule
{
  private readonly IOLabConfiguration _configuration;

  public FilesFilesystemModule(
    IOLabLogger logger,
    IOLabConfiguration configuration)
  {
    _configuration = configuration;

    logger.LogInformation($"Initializing FilesFilesystemModule");

    logger.LogInformation($"FileStorageFolder: {_configuration.GetAppSettings().FileStorageFolder}");
    logger.LogInformation($"FileStorageContainer: {_configuration.GetAppSettings().FileStorageContainer}");
    logger.LogInformation($"FileStorageUrl: {_configuration.GetAppSettings().FileStorageUrl}");

  }

  public char GetFolderSeparator() { return Path.DirectorySeparatorChar; }

  /// <summary>
  /// Attach URLs to access files
  /// </summary>
  /// <param name="items">SystemFiles records</param>
  public void AttachUrls(IOLabLogger logger, IList<SystemFiles> items)
  {
    logger.LogInformation($"Attaching file storage URLs for {items.Count} file records");

    foreach (var item in items)
    {
      var scopeLevel = item.ImageableType;
      var scopeId = item.ImageableId;

      var subPath = GetBasePath(scopeLevel, scopeId, item.Path);
      var physicalPath = GetPhysicalPath(scopeLevel, scopeId, item.Path);

      if (FileExists(logger, physicalPath, item.Path))
      {
        item.OriginUrl = $"{GetFolderSeparator()}{Path.GetFileName(_configuration.GetAppSettings().FileStorageFolder)}{GetFolderSeparator()}{subPath}";
        logger.LogInformation($"  '{item.Path}' mapped to url '{item.OriginUrl}'");
      }
      else
        item.OriginUrl = null;
    }
  }

  /// <summary>
  /// Move file from one location to another
  /// </summary>
  /// <param name="fileName">File name</param>
  /// <param name="sourcePath">Source path</param>
  /// <param name="destinationPath">Destination path</param>
  public void MoveFile(IOLabLogger logger,
    string fileName,
    string sourcePath,
    string destinationPath)
  {
    Guard.Argument(sourcePath).NotEmpty(nameof(sourcePath));
    Guard.Argument(destinationPath).NotEmpty(nameof(destinationPath));

    File.Move(sourcePath, destinationPath);

    logger.LogInformation($"moved file from '{sourcePath}' to {destinationPath}");
  }

  /// <summary>
  /// Test if file exists in storage
  /// </summary>
  /// <param name="physicalPath">Path to look for file</param>
  /// <returns>true/false</returns>
  public bool FileExists(
    IOLabLogger logger,
    string folderName,
    string physicalFileName )
  {
    var result = File.Exists($"{folderName}{GetFolderSeparator()}{physicalFileName}");
    if (!result)
      logger.LogWarning($"  '{folderName}{GetFolderSeparator()}{physicalFileName}' physical file not found");

    return result;
  }

  /// <summary>
  /// Uploads a file to upload directory
  /// </summary>
  /// <param name="file">File contents stream</param>
  /// <param name="fileName">(Optional) file name (temp name generated, if null)</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public async Task<string> UploadFileAsync(
    IOLabLogger logger,
    Stream file,
    string fileName,
    CancellationToken token)
  {
    if (string.IsNullOrEmpty(fileName))
      fileName = Path.GetRandomFileName();

    var physicalPath = Path.Combine(_configuration.GetAppSettings().ImportFolder, fileName);

    using (var stream = new FileStream(physicalPath, FileMode.Create))
    {
      await file.CopyToAsync(stream);
      logger.LogInformation($"uploaded file to '{physicalPath}'. Size: {file.Length}");
    }

    return physicalPath;
  }

  private string GetBasePath(string scopeLevel, uint scopeId, string filePath)
  {
    var subPath = $"{scopeLevel}{GetFolderSeparator()}{scopeId}{GetFolderSeparator()}{filePath}";
    return subPath;
  }

  private string GetPhysicalPath(string scopeLevel, uint scopeId, string filePath)
  {
    var subPath = GetBasePath(scopeLevel, scopeId, filePath);

    var physicalPath = Path.Combine(
      _configuration.GetAppSettings().FileStorageFolder,
      subPath.Replace('/', Path.DirectorySeparatorChar));
    return physicalPath;
  }

  public Task<string> SaveFile(IOLabLogger logger, string fileName, Stream stream, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public async Task<Stream> ReadFileAsync(IOLabLogger logger, 
      string folderName,
      string fileName)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Delete file from blob storage
  /// </summary>
  /// <param name="logger">OLabLogger</param>
  /// <param name="filePath">File to delete</param>
  /// <returns></returns>
  public async Task<bool> DeleteFileAsync(IOLabLogger logger, string filePath)
  {
    throw new NotImplementedException();
  }

  public async Task<bool> ExtractFileAsync(
    IOLabLogger logger,
    string folderName,
    string fileName,
    string extractPath,
    CancellationToken token)
  {
    throw new NotImplementedException();
  }
}

#pragma warning restore CS1998
