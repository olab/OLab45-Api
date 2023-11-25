using Dawn;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System.Configuration;
using System.IO.Compression;
using System.Text;

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
      var sourceFilePath = GetPhysicalPath(sourceFolder, fileName);

      if (FileExists(fileName, sourceFilePath))
        throw new Exception($"file '{sourceFilePath}' does not exist");

      if (!Directory.Exists(destinationFolder))
        Directory.CreateDirectory(destinationFolder);

      File.Move(
        sourceFilePath,
        Path.Combine(destinationFolder, fileName),
        true);

      logger.LogInformation($"moved {fileName} from '{sourceFilePath}' to {destinationFolder}");
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
  public override bool FileExists(
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

      using (var inputStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read))
      {
        inputStream.CopyTo(stream);

        stream.Position = 0;
        logger.LogInformation($"  read '{inputStream.Length}' bytes");
      }

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
  /// <param name="folderName">Target file folderName</param>
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
  /// <param name="folderName">Folder to delete</param>
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
    bool appendToStream,
    CancellationToken token = default)
  {
    Guard.Argument(archive).NotNull(nameof(archive));
    Guard.Argument(folderName).NotEmpty(nameof(folderName));

    var result = false;

    try
    {
      logger.LogInformation($"reading '{folderName}' for files to add to stream");

      var files = GetFiles(folderName);

      foreach (var file in files)
      {
        var physicalFilePath = GetPhysicalPath(file);

        using (var fileStream = new FileStream(physicalFilePath, FileMode.Open))
        {
          // need to take the file root off the path
          var entryName = file.Replace($"{FilesRoot}{GetFolderSeparator()}", "");

          logger.LogInformation($"  creating archive entry '{entryName}'");

          var entry = archive.CreateEntry(entryName);
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

      logger.LogInformation($"reading '{folderName}' for files");

      if (!Directory.Exists(physicalPath))
      {
        logger.LogInformation($"source folder '{folderName}' does not exist");
        return fileNames;
      }

      var contents = Directory.GetFiles(physicalPath).ToList();

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

}

#pragma warning restore CS1998
