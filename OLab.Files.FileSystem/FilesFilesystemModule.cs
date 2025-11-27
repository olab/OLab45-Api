using Dawn;
using OLab.Api.Model;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System.Configuration;
using System.IO.Compression;

namespace OLab.Files.FileSystem;

#pragma warning disable CS1998

/// <summary>
/// Module for handling file system operations.
/// </summary>
[OLabModule( "FILESYSTEM" )]
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
      IOLabConfiguration configuration) : base( logger, configuration )
  {
    // if not set to use this module, then don't proceed further
    if ( GetModuleName().ToLower() != cfg.GetAppSettings().FileStorageType.ToLower() )
      return;

    logger.LogInformation( $"Initializing FilesFilesystemModule" );

    if ( string.IsNullOrEmpty( cfg.GetAppSettings().FileStorageRoot ) )
      throw new ConfigurationErrorsException( "missing FileStorageRoot parameter" );

    if ( string.IsNullOrEmpty( cfg.GetAppSettings().FileStorageUrl ) )
      throw new ConfigurationErrorsException( "missing FileStorageRoot parameter" );

    if ( !Directory.Exists( cfg.GetAppSettings().FileStorageRoot ) )
      throw new ConfigurationErrorsException( $"{cfg.GetAppSettings().FileStorageRoot} root directory does not exist" );
  }

  /// <summary>
  /// Gets the folder separator character.
  /// </summary>
  /// <returns>The folder separator character.</returns>
  public override char GetFolderSeparator() { return Path.DirectorySeparatorChar; }

  /// <summary>
  /// Move file from one folder to another.
  /// </summary>
  /// <param name="relativeSourceFile">Relative source file path</param>
  /// <param name="destinationFolder">Relative destination path</param>
  /// <param name="token">Cancellation token</param>
  public override async Task MoveFileAsync(
      string relativeSourceFile,
      string destinationFolder,
      CancellationToken token = default)
  {
    Guard.Argument( relativeSourceFile ).NotEmpty( nameof( relativeSourceFile ) );
    Guard.Argument( destinationFolder ).NotEmpty( nameof( destinationFolder ) );

    try
    {
      var result = File.Exists( relativeSourceFile );
      if ( !result )
      {
        // not found, maybe a relative path was passed in
        relativeSourceFile = GetPhysicalPath( relativeSourceFile );
        result = File.Exists( relativeSourceFile );
        if ( !result )
          throw new Exception( $"file '{relativeSourceFile}' not found" );
      }

      var sourcePhysFilePath = relativeSourceFile;

      if ( !Directory.Exists( destinationFolder ) )
        Directory.CreateDirectory( destinationFolder );

      var destinationPhysFilePath = BuildPath(
          destinationFolder,
          Path.GetFileName( relativeSourceFile ) );

      File.Move(
          sourcePhysFilePath,
          destinationPhysFilePath,
          true );

      logger.LogInformation( $"moved '{sourcePhysFilePath}' to {destinationPhysFilePath}" );
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "MoveFileAsync Exception" );
      throw;
    }
  }

  /// <summary>
  /// Test if file exists in storage.
  /// </summary>
  /// <param name="filePath">Physical (or relative) path of file to look for</param>
  /// <returns>true if the file exists, false otherwise</returns>
  public override bool FileExists(
      string filePath)
  {
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      var result = File.Exists( filePath );
      if ( !result )
      {
        // not found, maybe a relative path was passed in
        filePath = GetPhysicalPath( filePath );
        result = File.Exists( filePath );
        if ( !result )
          logger.LogWarning( $"  '{filePath}' physical file not found" );
      }

      return result;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "FileExists Exception" );
      throw;
    }
  }

  /// <summary>
  /// Gets the public URL for the file.
  /// </summary>
  /// <param name="path">The scoped object path of the file</param>
  /// <param name="source">Files object</param>
  /// <returns>The public URL for the file</returns>
  public override SystemFiles UpdateUrlPath(
    string path,
    SystemFiles source)
  {
    var physicalPath = BuildPath(
        cfg.GetAppSettings().FileStorageUrl,
        FilesRoot,
        path,
        source.Path );

    source.OriginUrl = physicalPath.Replace( "\\", "/" );

    return source;
  }

  /// <summary>
  /// Gets the public URL for the file.
  /// </summary>
  /// <param name="path">The scoped object path of the file</param>
  /// <param name="source">Script object</param>
  /// <returns>The public URL for the file</returns>
  public override SystemScripts UpdateUrlPath(
    string path,
    SystemScripts source)
  {
    var physicalPath
      = BuildPath(
          cfg.GetAppSettings().FileStorageUrl,
          FilesRoot,
          path,
          source.Source );

    source.OriginUrl = physicalPath.Replace( "\\", "/" );

    return source;
  }

  /// <summary>
  /// Uploads a file represented by a stream to a directory.
  /// </summary>
  /// <param name="stream">File contents stream</param>
  /// <param name="relativeFile">Relative file name</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical file path</returns>
  public override async Task<string> WriteFileAsync(
      Stream stream,
      string relativeFile,
      CancellationToken token = default)
  {
    Guard.Argument( stream ).NotNull( nameof( stream ) );
    Guard.Argument( relativeFile ).NotEmpty( nameof( relativeFile ) );

    try
    {
      var physicalFile = GetPhysicalPath( relativeFile );

      var physicalDirectory = Path.GetDirectoryName( physicalFile );
      if ( !Directory.Exists( physicalDirectory ) )
        Directory.CreateDirectory( physicalDirectory );

      using ( var file = new FileStream( physicalFile, FileMode.OpenOrCreate, FileAccess.Write ) )
      {
        await stream.CopyToAsync( file );
        stream.Position = 0;
      }

      return relativeFile;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "WriteFileAsync Exception" );
      throw;
    }
  }

  /// <summary>
  /// Copy file presented by stream to file store.
  /// </summary>
  /// <param name="stream">File stream</param>
  /// <param name="filePath">Physical or relative file name</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>true if the file was read successfully, false otherwise</returns>
  public override async Task<bool> ReadFileAsync(
      Stream stream,
      string filePath,
      CancellationToken token = default)
  {
    Guard.Argument( stream ).NotNull( nameof( stream ) );
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      if ( !File.Exists( filePath ) )
      {
        filePath = GetPhysicalPath( filePath );
        if ( !File.Exists( filePath ) )
          return false;
      }

      logger.LogInformation( $"ReadFileAsync reading file '{filePath}'" );

      using var inputStream = new FileStream( filePath, FileMode.Open, FileAccess.Read );
      inputStream.CopyTo( stream );

      stream.Position = 0;
      logger.LogInformation( $"  read '{inputStream.Length}' bytes" );

      return true;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "ReadFileAsync Exception" );
      throw;
    }
  }

  /// <summary>
  /// Delete file.
  /// </summary>
  /// <param name="filePath">Physical or relative file to delete</param>
  /// <returns>true if the file was deleted successfully, false otherwise</returns>
  public override async Task<bool> DeleteFileAsync(
      string filePath)
  {
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      if ( !File.Exists( filePath ) )
      {
        filePath = GetPhysicalPath( filePath );
        if ( !File.Exists( filePath ) )
          return false;
      }

      logger.LogInformation( $"DeleteFileAsync deleting '{filePath}'" );

      File.Delete( filePath );
      return true;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "CopyStreamToFileAsync Exception" );
      throw;
    }
  }

  /// <summary>
  /// Delete folder from blob storage.
  /// </summary>
  /// <param name="relativeFolderName">Folder to delete</param>
  public override async Task DeleteFolderAsync(
      string relativeFolderName)
  {
    var physicalFolderName = GetPhysicalPath( relativeFolderName );
    if ( Directory.Exists( physicalFolderName ) )
      Directory.Delete( physicalFolderName, true );
  }

  /// <summary>
  /// Extract archive file to folder.
  /// </summary>
  /// <param name="relativeArchiveFile">Archive file folder</param>
  /// <param name="relativeExtractDirectory">Destination decompress folder</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>Physical path of the extracted directory</returns>
  public override async Task<string> ExtractFileToStorageAsync(
      string relativeArchiveFile,
      string relativeExtractDirectory,
      CancellationToken token = default)
  {
    Guard.Argument( relativeArchiveFile ).NotEmpty( nameof( relativeArchiveFile ) );
    Guard.Argument( relativeExtractDirectory ).NotEmpty( nameof( relativeExtractDirectory ) );

    try
    {
      logger.LogInformation( $"extracting {relativeArchiveFile} -> {relativeExtractDirectory}" );

      await DeleteFolderAsync( relativeExtractDirectory );

      var physicalExtractDirectory = GetPhysicalPath( relativeExtractDirectory );

      ZipFile.ExtractToDirectory(
          GetPhysicalPath( relativeArchiveFile ),
          physicalExtractDirectory );

      return physicalExtractDirectory;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "ExtractFileToStorageAsync error" );
      throw;
    }
  }

  /// <summary>
  /// Create archive file from a folder.
  /// </summary>
  /// <param name="archive">Archive file stream</param>
  /// <param name="folderName">Source file folder</param>
  /// <param name="zipEntryFolderName">Zip entry folder name</param>
  /// <param name="appendToStream">Append or replace stream contents</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>true if the folder was copied to the archive successfully, false otherwise</returns>
  public override async Task<bool> CopyFolderToArchiveAsync(
      ZipArchive archive,
      string folderName,
      string zipEntryFolderName,
      bool appendToStream,
      CancellationToken token = default)
  {
    Guard.Argument( archive ).NotNull( nameof( archive ) );
    Guard.Argument( folderName ).NotEmpty( nameof( folderName ) );

    var result = false;

    try
    {
      var files = GetFiles( folderName );

      foreach ( var file in files )
      {
        var physicalFilePath = GetPhysicalPath( file );

        using var fileStream = new FileStream( physicalFilePath, FileMode.Open );
        var entryPath = BuildPath( zipEntryFolderName, Path.GetFileName( file ) );
        // normalize to standard folder separator
        entryPath = entryPath.Replace( '\\', '/' );

        logger.LogInformation( $"  adding '{file}' to archive '{entryPath}'. size = {fileStream.Length}" );

        var entry = archive.CreateEntry( entryPath );
        using var entryStream = entry.Open();
        fileStream.CopyTo( entryStream );
        entryStream.Close();
      }
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "CopyFolderToArchiveAsync error" );
      throw;
    }

    return result;
  }

  /// <summary>
  /// Get list of files in a folder.
  /// </summary>
  /// <param name="folderName">Folder name</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>List of file names</returns>
  public override IList<string> GetFiles(
      string folderName,
      CancellationToken token = default)
  {
    var fileNames = new List<string>();

    try
    {
      logger.LogInformation( $"Get files listing for folder '{folderName}'" );

      var physicalPath = GetPhysicalPath( folderName );

      if ( !Directory.Exists( physicalPath ) )
        return fileNames;

      var contents = Directory.GetFiles( physicalPath ).ToList();

      if ( contents.Count > 0 )
        logger.LogInformation( $"found {contents.Count} files in '{folderName}'" );

      fileNames = contents.Select( x => BuildPath( folderName, Path.GetFileName( x ) ) ).ToList();
      foreach ( var fileName in fileNames )
        logger.LogInformation( $"  {fileName}" );

      return fileNames;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "GetFiles error" );
      throw;
    }
  }
}

#pragma warning restore CS1998
