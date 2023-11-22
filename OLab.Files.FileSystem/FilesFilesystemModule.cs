using Dawn;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.IO.Compression;

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

  public string GetModuleName()
  {
    var attrib = this.GetType().GetCustomAttributes(typeof(OLabModuleAttribute), true).FirstOrDefault() as OLabModuleAttribute;
    return attrib == null ? "" : attrib.Name;
  }

  private string GetPhysicalPath(string folderName, string fileName)
  {
    var physicalPath = Path.Combine(
      _configuration.GetAppSettings().FileStorageFolder,
      folderName,
      fileName);
    return physicalPath;
  }

  private string GetPhysicalPath(string filePath)
  {
    var physicalPath = Path.Combine(
      _configuration.GetAppSettings().FileStorageFolder,
      filePath.Replace('/', GetFolderSeparator()));
    return physicalPath;
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
      try
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var physicalPath = GetPhysicalPath($"{scopeLevel}{GetFolderSeparator()}{scopeId}{GetFolderSeparator()}{item.Path}");

        if (FileExists(Path.GetDirectoryName(physicalPath), item.Path))
        {
          item.OriginUrl = $"/{Path.GetFileName(_configuration.GetAppSettings().FileStorageFolder)}/{scopeLevel}/{scopeId}/{item.Path}";
          logger.LogInformation($"  file {item.Name}({item.Id}): '{item.Path}' mapped to url '{item.OriginUrl}'");
        }
        else
        {
          logger.LogWarning($"  '{physicalPath}' not found");
          item.OriginUrl = null;
        }
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "AttachUrls Exception");
      }
    }


  }

  /// <summary>
  /// Move file from one folder to another
  /// </summary>
  /// <param name="fileName">File name</param>
  /// <param name="sourceFolder">Source path</param>
  /// <param name="destinationFolder">Destination path</param>
  public async Task MoveFileAsync(
      string fileName,
      string sourceFolder,
      string destinationFolder,
      CancellationToken token = default)
  {
    Guard.Argument(sourceFolder).NotEmpty(nameof(sourceFolder));
    Guard.Argument(destinationFolder).NotEmpty(nameof(destinationFolder));

    try
    {
      var sourceFilePath = GetPhysicalPath(sourceFolder, fileName);
      File.Move(
        sourceFilePath,
        destinationFolder);

      logger.LogInformation($"moved file from '{sourceFilePath}' to {destinationFolder}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "MoveFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Test if file exists in storage
  /// </summary>
  /// <param name="physicalPath">Path to look for file</param>
  /// <returns>true/false</returns>
  public bool FileExists(
    string folderName,
    string fileName)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalPath = GetPhysicalPath(folderName, fileName);
      var result = File.Exists(physicalPath);
      if (!result)
        logger.LogWarning($"  '{physicalPath}' physical file not found");

      return result;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "FileExists Exception");
      throw;
    }

  }

  /// <summary>
  /// Uploads a file represented by a stream to a directory
  /// </summary>
  /// <param name="file">File contents stream</param>
  /// <param name="targetFolder">Target folderName</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public async Task<string> CopyStreamToFileAsync(
    Stream stream,
    string targetFolder,
    CancellationToken token)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(targetFolder).NotEmpty(nameof(targetFolder));

    try
    {
      var physicalPath = GetPhysicalPath(targetFolder);

      logger.LogInformation($"Writing physical file: {physicalPath}");

      var physicalDirectory = Path.GetDirectoryName(physicalPath);
      if (physicalDirectory != null)
        Directory.CreateDirectory(physicalDirectory);

      using (var file = new FileStream(physicalPath, FileMode.Create))
      {
        await stream.CopyToAsync(file);
        logger.LogInformation($"wrote file to '{physicalPath}'. Size: {file.Length}");
      }

      return physicalPath;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyStreamToFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Copy file presented by stream to file store
  /// </summary>
  /// <param name="stream">File stream</param>
  /// <param name="folderName">Target folder</param>
  /// <param name="fileName">Target file name</param>
  /// <param name="token"></param>
  public async Task CopyFileToStreamAsync(
      Stream stream,
      string folderName,
      string fileName,
      CancellationToken token)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {

      var physicalFilePath = GetPhysicalPath(folderName, fileName);

      logger.LogInformation($"CopyStreamToFileAsync writing to file '{physicalFilePath}'");

      using (var inputStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read))
      {
        inputStream.CopyTo(stream);

        stream.Position = 0;
        logger.LogInformation($"  wrote '{inputStream.Length}'");
      }

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyFileToStreamAsync Exception");
      throw;
    }
  }

  /// <summary>
  /// Delete file
  /// </summary>
  /// <param name="folderName">Target file folderName</param>
  /// <param name="fileName">File name</param>
  /// <returns></returns>
  public async Task<bool> DeleteFileAsync(
  string folderName,
  string fileName)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalFilePath = GetPhysicalPath(folderName, fileName);
      logger.LogInformation($"DeleteFileAsync deleting '{physicalFilePath}'");

      File.Delete(physicalFilePath);
      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyStreamToFileAsync Exception");
      throw;
    }
  }

  /// <summary>
  /// Extract archive file to folder
  /// </summary>
  /// <param name="folderName">Archive file folder</param>
  /// <param name="fileName">Archive file name</param>
  /// <param name="token"></param>
  public async Task<bool> ExtractFileToStorageAsync(
    string folderName,
    string fileName,
    string extractDirectoryName,
    CancellationToken token)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));
    Guard.Argument(extractDirectoryName).NotEmpty(nameof(extractDirectoryName));

    try
    {

      logger.LogInformation($"extracting '{folderName}' {fileName} -> {extractDirectoryName}");

      var archiveFilePath = GetPhysicalPath(folderName, fileName);
      var extractPath = GetPhysicalPath(extractDirectoryName);

      if (Directory.Exists(extractPath))
        Directory.Delete(extractPath, true);

      ZipFile.ExtractToDirectory(archiveFilePath, extractPath);
      return true;

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ExtractFileToStorageAsync error");
      throw;
    }

  }

  /// <summary>
  /// Create archvie file from a folder
  /// </summary>
  /// <param name="archive">Archive file stream</param>
  /// <param name="folderName">Source file folder</param>
  /// <param name="appendToStream">Append or replace stream contents</param>
  /// <param name="token"></param>
  public async Task<bool> CopyFoldertoArchiveAsync(
    ZipArchive archive,
    string folderName,
    bool appendToStream,
    CancellationToken token)
  {
    Guard.Argument(archive).NotNull(nameof(archive));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));

    var result = false;

    try
    {
      logger.LogInformation($"reading '{folderName}' for files to add to stream");

      var physicalPath = GetPhysicalPath(folderName);

      // check if sirectory exists
      if (!Directory.Exists(physicalPath))
      {
        logger.LogInformation($"  file folder '{physicalPath}' does not exist");
        return result;
      }

      var files = Directory.GetFiles(physicalPath);

      foreach (var file in files)
      {
        using (var fileStream = new FileStream(file, FileMode.Open))
        {
          var entryPath = $"{folderName}{GetFolderSeparator()}{Path.GetFileName(file)}";

          logger.LogInformation($"  adding '{file}' to archive '{entryPath}'");

          var entry = archive.CreateEntry(entryPath);
          using (var entryStream = entry.Open())
          {
            fileStream.CopyTo(entryStream);
            entryStream.Close();
          }

        }

      }

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyFoldertoArchiveAsync error");
      throw;
    }


    return result;
  }
}

#pragma warning restore CS1998
