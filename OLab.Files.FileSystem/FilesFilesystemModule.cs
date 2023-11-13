using Dawn;
using HttpMultipartParser;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.IO;
using static NuGet.Packaging.PackagingConstants;

namespace OLab.Files.FileSystem;

#pragma warning disable CS1998

[OLabModule("FILESYSTEM")]
public class FilesFilesystemModule : IFileStorageModule
{
  private readonly IOLabLogger logger;
  private readonly IOLabConfiguration _configuration;

  public FilesFilesystemModule(
    IOLabLogger logger,
    IOLabConfiguration configuration)
  {
    this.logger = logger;
    _configuration = configuration;

    // if not set to use this module, then don't proceed further
    if (GetModuleName().ToLower() != _configuration.GetAppSettings().FileStorageType.ToLower())
      return;

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
  public void AttachUrls(IList<SystemFiles> items)
  {
    logger.LogInformation($"Attaching file storage URLs for {items.Count} file records");

    foreach (var item in items)
    {
      var scopeLevel = item.ImageableType;
      var scopeId = item.ImageableId;

      var physicalPath = GetPhysicalPath(item.Path);

      if (FileExists(physicalPath, item.Path))
      {
        item.OriginUrl = $"{GetFolderSeparator()}{Path.GetFileName(_configuration.GetAppSettings().FileStorageFolder)}{GetFolderSeparator()}{scopeLevel}{GetFolderSeparator()}{scopeId}{item.Path}";
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
  public async Task MoveFileAsync(
      string fileName,
      string sourcePath,
      string destinationPath,
      CancellationToken token = default)
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
    string folderName,
    string physicalFileName)
  {
    var result = File.Exists($"{folderName}{GetFolderSeparator()}{physicalFileName}");
    if (!result)
      logger.LogWarning($"  '{folderName}{GetFolderSeparator()}{physicalFileName}' physical file not found");

    return result;
  }

  /// <summary>
  /// Uploads an import file to upload directory
  /// </summary>
  /// <param name="file">File contents stream</param>
  /// <param name="fileName">(Optional) file name (temp name generated, if null)</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public async Task<string> WriteFileAsync(
    Stream stream,
    string targetFilePath,
    CancellationToken token)
  {
    var fullFileName = GetPhysicalPath(targetFilePath); 

    logger.LogInformation($"Write physical file: {fullFileName}");

    var physicalPath = Path.GetDirectoryName(fullFileName);
    if (physicalPath != null)
      Directory.CreateDirectory(physicalPath);

    using (var file = new FileStream(fullFileName, FileMode.Create))
    {
      await stream.CopyToAsync(file);
      logger.LogInformation($"write file to '{fullFileName}'. Size: {file.Length}");
    }

    return fullFileName;
  }

  private string GetPhysicalPath(string filePath)
  {
    var physicalPath = Path.Combine(
      _configuration.GetAppSettings().FileStorageFolder,
      filePath.Replace('/', Path.DirectorySeparatorChar));
    return physicalPath;
  }

  public Task<string> SaveFile(string fileName, Stream stream, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public async Task ReadFileAsync(
      Stream stream,
      string folder,
      string fileName,
      CancellationToken token)
  {
    try
    {
      var filePath = $"{folder}{GetFolderSeparator()}{fileName}";

      logger.LogInformation($"ReadFileAsync reading '{filePath}'");

      using (var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
      {
        inputStream.CopyTo(stream);
        stream.Position = 0;
        logger.LogInformation($"  read '{inputStream.Length}'");
      }

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
  /// <param name="filePath">File to delete</param>
  /// <returns></returns>
  public async Task<bool> DeleteFileAsync(
  string folder,
  string fileName)
  {
    try
    {
      var filePath = $"{folder}{GetFolderSeparator()}{fileName}";
      logger.LogInformation($"DeleteFileAsync deleting '{filePath}'");

      File.Delete(filePath);
      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ReadFileAsync Exception");
      throw;
    }
  }

  public async Task<bool> ExtractFileAsync(
    string folderName,
    string fileName,
    string extractDirectoryName,
    CancellationToken token)
  {
    logger.LogInformation($"extracting '{folderName}' {fileName} -> {extractDirectoryName}");

    var archiveFilePath = $"{folderName}{GetFolderSeparator()}{fileName}";
    var extractPath  = $"{folderName}{GetFolderSeparator()}{extractDirectoryName}";

    if ( Directory.Exists(extractPath) )
      Directory.Delete(extractPath, true);

    System.IO.Compression.ZipFile.ExtractToDirectory(archiveFilePath, extractPath);
    return true;
  }

  public string GetModuleName()
  {
    var attrib = this.GetType().GetCustomAttributes(typeof(OLabModuleAttribute), true).FirstOrDefault() as OLabModuleAttribute;
    return attrib == null ? "" : attrib.Name;
  }
}

#pragma warning restore CS1998
