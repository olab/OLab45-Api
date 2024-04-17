using Dawn;
using Humanizer;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System.Configuration;
using System.IO.Compression;

namespace OLab.Files.FileSystem;

#pragma warning disable CS1998

[OLabModule("FILESYSTEM")]
public class FilesFilesystemModule : OLabFileStorageModule
{
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="logger">OlabLogger</param>
  /// <param name="configuration">Application cfg</param>
  /// <exception cref="ConfigurationErrorsException"></exception>
  public FilesFilesystemModule(
    IOLabLogger logger,
    IOLabConfiguration configuration) : base(logger, configuration)
  {
    // if not set to use this module, then don't proceed further
    if (GetModuleName().ToLower() != cfg.GetAppSettings().FileStorageType.ToLower())
      return;

    logger.LogInformation($"Initializing FilesFilesystemModule");

    if (string.IsNullOrEmpty(cfg.GetAppSettings().FileStorageRoot))
      throw new ConfigurationErrorsException("missing FileStorageRoot parameter");

    if (string.IsNullOrEmpty(cfg.GetAppSettings().FileStorageUrl))
      throw new ConfigurationErrorsException("missing FileStorageRoot parameter");

    if (!Directory.Exists(cfg.GetAppSettings().FileStorageRoot))
      throw new ConfigurationErrorsException($"{cfg.GetAppSettings().FileStorageRoot} root directory does not exist");
  }

  public override char GetFolderSeparator() { return Path.DirectorySeparatorChar; }

  /// <summary>
  /// Move file from one folder to another
  /// </summary>
  /// <param name="relativeSourceFilePath">Relative source file path</param>
  /// <param name="relativeDestinationFolder">Relative destination path</param>
  public override async Task MoveFileAsync(
      string relativeSourceFilePath,
      string relativeDestinationFolder,
      CancellationToken token = default)
  {
    Guard.Argument(relativeSourceFilePath).NotEmpty(nameof(relativeSourceFilePath));
    Guard.Argument(relativeDestinationFolder).NotEmpty(nameof(relativeDestinationFolder));

    try
    {
      if (!FileExists(relativeSourceFilePath))
        throw new Exception($"file '{relativeSourceFilePath}' not found");

      var sourcePhysFilePath = GetPhysicalPath(relativeSourceFilePath);
      var destinationPhysFolder = GetPhysicalPath(relativeDestinationFolder);

      if (!Directory.Exists(destinationPhysFolder))
        Directory.CreateDirectory(destinationPhysFolder);

      var destinationPhysFilePath = GetPhysicalPath(
        BuildPath( 
          relativeDestinationFolder, 
          Path.GetFileName(relativeSourceFilePath)));

      File.Move(
        sourcePhysFilePath,
        destinationPhysFilePath,
        true);

      logger.LogInformation($"moved '{sourcePhysFilePath}' to {destinationPhysFilePath}");
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
  /// <param name="relativeFilePath">Relative path of file to look for</param>
  /// <returns>true/false</returns>
  public override bool FileExists(
    string relativeFilePath)
  {
    Guard.Argument(relativeFilePath).NotEmpty(nameof(relativeFilePath));

    try
    {
      var physicalPath = GetPhysicalPath(relativeFilePath);

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
  /// Gets the public URL for the file
  /// </summary>
  /// <param name="path"></param>
  /// <param name="fileName"></param>
  /// <returns></returns>
  public override string GetUrlPath(string path, string fileName)
  {
    var physicalPath = BuildPath(
      path,
      fileName);

    physicalPath = physicalPath.Replace("\\", "/");
 
    return physicalPath;
  }

  /// <summary>
  /// Uploads a file represented by a stream to a directory
  /// </summary>
  /// <param name="file">File contents stream</param>
  /// <param name="targetFolder">Relative target file path</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public override async Task<string> WriteFileAsync(
    Stream stream,
    string relativeFilePath,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(relativeFilePath).NotEmpty(nameof(relativeFilePath));

    try
    {
      var physicalFileName = GetPhysicalPath(relativeFilePath);
      var physicalFileDirectory = Path.GetDirectoryName(physicalFileName);

      logger.LogInformation($"Writing file {relativeFilePath} to {physicalFileName}");

      if (Directory.Exists(physicalFileDirectory))
        Directory.Delete(physicalFileDirectory, true);

      Directory.CreateDirectory(physicalFileDirectory);

      using (var file = new FileStream(physicalFileName, FileMode.OpenOrCreate, FileAccess.Write))
      {
        await stream.CopyToAsync(file);
        logger.LogInformation($"wrote to file '{physicalFileName}'. Size: {file.Length}");
      }

      return relativeFilePath;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "WriteFileAsync Exception");
      throw;
    }

  }

  /// <summary>
  /// Copy file presented by stream to file store
  /// </summary>
  /// <param name="stream">File stream</param>
  /// <param name="relativeFilePath">Relative file name</param>
  /// <param name="token"></param>
  public override async Task ReadFileAsync(
    Stream stream,
    string relativeFilePath,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(relativeFilePath).NotEmpty(nameof(relativeFilePath));

    try
    {

      var physicalFilePath = GetPhysicalPath(relativeFilePath);
      logger.LogInformation($"ReadFileAsync reading file '{physicalFilePath}'");

      using var inputStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read);
      inputStream.CopyTo(stream);

      stream.Position = 0;
      logger.LogInformation($"  read '{inputStream.Length}' bytes");

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ReadFileAsync Exception");
      throw;
    }
  }

  /// <summary>
  /// Delete file
  /// </summary>
  /// <param name="relativeFilePath">Relative file apth</param>
  /// <returns></returns>
  public override async Task<bool> DeleteFileAsync(
    string relativeFilePath)
  {
    Guard.Argument(relativeFilePath).NotEmpty(nameof(relativeFilePath));

    try
    {
      var physicalFilePath = GetPhysicalPath(relativeFilePath);
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
  /// Delete folder from blob storage
  /// </summary>
  /// <param name="relativePath">Folder to delete</param>
  public override async Task DeleteFolderAsync(
    string relativeFolderName)
  {
    var folderPath = GetPhysicalPath(relativeFolderName);
    if (Directory.Exists(folderPath))
      Directory.Delete(folderPath, true);
  }

  /// <summary>
  /// Extract archive file to folder
  /// </summary>
  /// <param name="relativeArchiveFilePath">RElative archive file folder</param>
  /// <param name="relativeExtractDirectory">Destination decompress folder</param>
  /// <param name="token"></param>
  public override async Task<bool> ExtractFileToStorageAsync(
    string relativeArchiveFilePath,
    string relativeExtractDirectory,
    CancellationToken token = default)
  {
    Guard.Argument(relativeArchiveFilePath).NotEmpty(nameof(relativeArchiveFilePath));
    Guard.Argument(relativeExtractDirectory).NotEmpty(nameof(relativeExtractDirectory));

    try
    {

      logger.LogInformation($"extracting {relativeArchiveFilePath} -> {relativeExtractDirectory}");

      var archiveFilePath = GetPhysicalPath(relativeArchiveFilePath);
      var extractPath = GetPhysicalPath(relativeExtractDirectory);

      await DeleteFolderAsync(extractPath);

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
  /// <param name="relativeFileDirectory">Relative source file folder</param>
  /// <param name="appendToStream">Append or replace stream contents</param>
  /// <param name="token"></param>
  public override async Task<bool> CopyFolderToArchiveAsync(
    ZipArchive archive,
    string relativeFileDirectory,
    string zipEntryFolderName,
    bool appendToStream,
    CancellationToken token = default)
  {
    Guard.Argument(archive).NotNull(nameof(archive));
    Guard.Argument(relativeFileDirectory).NotEmpty(nameof(relativeFileDirectory));

    var result = false;

    try
    {
      var files = GetFiles(relativeFileDirectory);

      foreach (var file in files)
      {
        var physicalFilePath = GetPhysicalPath(file);

        using var fileStream = new FileStream(physicalFilePath, FileMode.Open);
        var entryPath = BuildPath(zipEntryFolderName, Path.GetFileName(file));
        // normalize to standard folder separator
        entryPath = entryPath.Replace('\\', '/');

        logger.LogInformation($"  adding '{file}' to archive '{entryPath}'. size = {fileStream.Length}");

        var entry = archive.CreateEntry(entryPath);
        using var entryStream = entry.Open();
        fileStream.CopyTo(entryStream);
        entryStream.Close();

      }

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CopyFolderToArchiveAsync error");
      throw;
    }


    return result;
  }

  /// <summary>
  /// Get list of files in directory
  /// </summary>
  /// <param name="relativeFileDirectory">Relative directory</param>
  /// <param name="token"></param>
  /// <returns></returns>
  public override IList<string> GetFiles(
    string relativeFileDirectory,
    CancellationToken token = default)
  {
    var fileNames = new List<string>();

    try
    {
      var physicalPath = GetPhysicalPath(relativeFileDirectory);

      if (!Directory.Exists(physicalPath))
      {
        logger.LogInformation($"source folder '{relativeFileDirectory}' does not exist");
        return fileNames;
      }

      var contents = Directory.GetFiles(physicalPath).ToList();

      if (contents.Count > 0)
        logger.LogInformation($"found {contents.Count} files in '{relativeFileDirectory}'");

      fileNames = contents.Select(x => BuildPath(relativeFileDirectory, Path.GetFileName(x))).ToList();
      foreach (var fileName in fileNames)
        logger.LogInformation($"  {fileName}");

      return fileNames;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetFiles error");
      throw;
    }

  }

  /// Calculate physical target directory for scoped type and id
  /// </summary>
  /// <param name="parentType">Scoped object type (e.g. 'Maps')</param>
  /// <param name="parentId">Scoped object id</param>
  /// <param name="fileName">Optional file name</param>
  /// <returns>Public directory for scope</returns>
  //public override string GetPublicFileDirectory(string parentType, uint parentId, string fileName = "")
  //{
  //  var targetDirectory = BuildPath(FilesRoot, parentType, parentId.ToString());

  //  if (!string.IsNullOrEmpty(fileName))
  //    targetDirectory = $"{targetDirectory}{GetFolderSeparator()}{fileName}";

  //  return targetDirectory;
  //}
}

#pragma warning restore CS1998
