using Dawn;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
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
  /// <param name="fileName">File name</param>
  /// <param name="sourceFolder">Source path</param>
  /// <param name="destinationFolder">Destination path</param>
  public override async Task MoveFileAsync(
      string fileName,
      string sourceFolder,
      string destinationFolder,
      CancellationToken token = default)
  {
    Guard.Argument(sourceFolder).NotEmpty(nameof(sourceFolder));
    Guard.Argument(destinationFolder).NotEmpty(nameof(destinationFolder));

    try
    {
      if (!FileExists(sourceFolder, fileName))
        throw new Exception($"file '{fileName}' not found");

      var sourcePhysFilePath = GetPhysicalPath(BuildPath(sourceFolder, fileName));
      var destinationPhysFolder = GetPhysicalPath(destinationFolder);

      if (!Directory.Exists(destinationPhysFolder))
        Directory.CreateDirectory(destinationPhysFolder);

      var destinationPhysFilePath = GetPhysicalPath(destinationFolder, fileName);

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
  /// <param name="relativePath">Relative path to look for</param>
  /// <param name="fileName">File name to look for</param>
  /// <returns>true/false</returns>
  public override bool FileExists(
    string relativePath,
    string fileName)
  {
    Guard.Argument(relativePath).NotEmpty(nameof(relativePath));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalPath = GetPhysicalPath(relativePath, fileName);
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
  /// <param name="targetFolder">Target relativePath</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public override async Task<string> WriteFileAsync(
    Stream stream,
    string folderName,
    string fileName,
    CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {
      var physicalPath = GetPhysicalPath(folderName);
      logger.LogInformation($"Writing file {fileName} to {physicalPath}");

      if (!Directory.Exists(physicalPath))
        Directory.CreateDirectory(physicalPath);

      var physicalFileName = BuildPath(
        physicalPath,
        fileName);

      using (var file = new FileStream(physicalFileName, FileMode.OpenOrCreate, FileAccess.Write))
      {
        await stream.CopyToAsync(file);
        logger.LogInformation($"wrote to file '{physicalFileName}'. Size: {file.Length}");
      }

      return physicalPath;
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
  /// <param name="folderName">Target folder</param>
  /// <param name="fileName">Target file name</param>
  /// <param name="token"></param>
  public override async Task ReadFileAsync(
      Stream stream,
      string folderName,
      string fileName,
      CancellationToken token = default)
  {
    Guard.Argument(stream).NotNull(nameof(stream));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));

    try
    {

      var physicalFilePath = GetPhysicalPath(folderName, fileName);
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
  /// <param name="folderName">Target file relativePath</param>
  /// <param name="fileName">File name</param>
  /// <returns></returns>
  public override async Task<bool> DeleteFileAsync(
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

  /// Delete folder from blob storage
  /// </summary>
  /// <param name="relativePath">Folder to delete</param>
  public override async Task DeleteFolderAsync(string folderName)
  {
    if (Directory.Exists(folderName))
      Directory.Delete(folderName, true);
  }

  /// <summary>
  /// Extract archive file to folder
  /// </summary>
  /// <param name="folderName">Archive file folder</param>
  /// <param name="fileName">Archive file name</param>
  /// <param name="token"></param>
  public override async Task<bool> ExtractFileToStorageAsync(
    string folderName,
    string fileName,
    string extractDirectoryName,
    CancellationToken token = default)
  {
    Guard.Argument(folderName).NotEmpty(nameof(folderName));
    Guard.Argument(fileName).NotEmpty(nameof(fileName));
    Guard.Argument(extractDirectoryName).NotEmpty(nameof(extractDirectoryName));

    try
    {

      logger.LogInformation($"extracting '{folderName}' {fileName} -> {extractDirectoryName}");

      var archiveFilePath = GetPhysicalPath(folderName, fileName);
      var extractPath = GetPhysicalPath(extractDirectoryName);

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
  /// <param name="folderName">Source file folder</param>
  /// <param name="appendToStream">Append or replace stream contents</param>
  /// <param name="token"></param>
  public override async Task<bool> CopyFolderToArchiveAsync(
    ZipArchive archive,
    string folderName,
    string zipEntryFolderName,
    bool appendToStream,
    CancellationToken token = default)
  {
    Guard.Argument(archive).NotNull(nameof(archive));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));

    var result = false;

    try
    {
      var files = GetFiles(folderName);

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

  public override IList<string> GetFiles(
    string folderName,
    CancellationToken token = default)
  {
    var fileNames = new List<string>();

    try
    {
      var physicalPath = GetPhysicalPath(folderName);

      if (!Directory.Exists(physicalPath))
      {
        logger.LogInformation($"source folder '{folderName}' does not exist");
        return fileNames;
      }

      var contents = Directory.GetFiles(physicalPath).ToList();

      if (contents.Count > 0)
        logger.LogInformation($"found {contents.Count} files in '{folderName}'");

      fileNames = contents.Select(x => BuildPath(folderName, Path.GetFileName(x))).ToList();
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

  /// Calculate target directory for scoped type and id
  /// </summary>
  /// <param name="parentType">Scoped object type (e.g. 'Maps')</param>
  /// <param name="parentId">Scoped object id</param>
  /// <param name="fileName">Optional file name</param>
  /// <returns>Public directory for scope</returns>
  public override string GetPublicFileDirectory(string parentType, uint parentId, string fileName = "")
  {
    var targetDirectory = BuildPath(parentType, parentId.ToString());

    if (!string.IsNullOrEmpty(fileName))
      targetDirectory = $"{targetDirectory}{GetFolderSeparator()}{fileName}";

    return targetDirectory;
  }
}

#pragma warning restore CS1998
