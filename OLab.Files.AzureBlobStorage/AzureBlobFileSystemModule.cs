using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dawn;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using System.Configuration;
using System.IO.Compression;

namespace OLab.Files.AzureBlobStorage;

[OLabModule( "AZUREBLOBSTORAGE" )]
/// <summary>
/// AzureBlobFileSystemModule is a file storage module that interacts with Azure Blob Storage.
/// It provides methods to perform file operations such as reading, writing, moving, and deleting files.
/// </summary>
public class AzureBlobFileSystemModule : OLabFileStorageModule
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly string _containerName;

  private readonly Dictionary<string, IList<BlobItem>>
    _folderContentCache = new();

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="logger">OlabLogger</param>
  /// <param name="configuration">Application cfg</param>
  /// <exception cref="ConfigurationErrorsException"></exception>
  public AzureBlobFileSystemModule(
    IOLabLogger logger,
    IOLabConfiguration configuration) : base( logger, configuration )
  {
    // if not set to use this module, then don't proceed further
    if ( GetModuleName().ToLower() != cfg.GetAppSettings().FileStorageType.ToLower() )
      return;

    logger.LogInformation( $"Initializing AzureBlobFileSystemModule" );

    var connectionString = cfg.GetAppSettings().FileStorageConnectionString;
    if ( string.IsNullOrEmpty( connectionString ) )
      throw new ConfigurationErrorsException( "missing FileStorageConnectionString parameter" );
    _blobServiceClient = new BlobServiceClient( connectionString );

    _containerName = Path.GetDirectoryName( cfg.GetAppSettings().FileStorageRoot );
    if ( string.IsNullOrEmpty( _containerName ) )
      throw new ConfigurationErrorsException( "missing FileStorageRoot parameter" );

    logger.LogInformation( $"Container: {_containerName}" );

    // need to prevent container name from being part of the file root
    cfg.GetAppSettings().FileStorageRoot = Path.GetFileName( cfg.GetAppSettings().FileStorageRoot );
  }

  /// <summary>
  /// Gets the folder separator character.
  /// </summary>
  /// <returns>The folder separator character.</returns>
  public override char GetFolderSeparator() { return '/'; }

  /// <summary>
  /// Gets the file path components.
  /// </summary>
  /// <param name="filePath">The file path.</param>
  /// <returns>A tuple containing the container and path.</returns>
  public (string container, string path) GetFilePath(string filePath)
  {
    var pathParts = filePath.Split( GetFolderSeparator() );
    // remove 1st part of the path, which is probably the container
    var folder = string.Join( GetFolderSeparator().ToString(), pathParts.Skip( 1 ) );

    return (pathParts[ 0 ], folder);
  }

  /// <summary>
  /// Tests if a file exists.
  /// </summary>
  /// <param name="filePath">Relative to root file path.</param>
  /// <returns>true if the file exists; otherwise, false.</returns>
  public override bool FileExists(
    string filePath)
  {
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    var result = false;

    try
    {
      IList<BlobItem> blobs;

      // if we do not have this sourceFolderName already in cache
      // then hit the blob storage and cache the results
      if ( !_folderContentCache.ContainsKey( filePath ) )
      {
        logger.LogInformation( $"  searching '{filePath} for blobs'" );

        blobs = _blobServiceClient
          .GetBlobContainerClient( _containerName )
          .GetBlobs( prefix: filePath ).ToList();

        _folderContentCache[ filePath ] = blobs;

        foreach ( var blob in blobs )
          logger.LogInformation( $"  found blob '{blob.Name}'" );

      }
      else
        blobs = _folderContentCache[ filePath ];

      result = blobs.Any( x => x.Name.Contains( Path.GetFileName( filePath ) ) );

      if ( !result )
        logger.LogWarning( $"  '{filePath}' not found" );
      else
        logger.LogInformation( $"  '{filePath}' exists" );

    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "FileExists error" );
      throw;
    }

    return result;
  }

  /// <summary>
  /// Moves a file between folders.
  /// </summary>
  /// <param name="sourceFilePath">Relative to root file path.</param>
  /// <param name="destinationFolder">Relative to root destination folder name.</param>
  /// <param name="token">Cancellation token.</param>
  public override async Task MoveFileAsync(
    string sourceFilePath,
    string destinationFolder,
    CancellationToken token = default)
  {
    Guard.Argument( sourceFilePath ).NotEmpty( nameof( sourceFilePath ) );
    Guard.Argument( destinationFolder ).NotEmpty( nameof( destinationFolder ) );

    try
    {
      logger.LogInformation( $"MoveFileAsync '{sourceFilePath} -> {destinationFolder}" );

      using var stream = new MemoryStream();

      await ReadFileAsync( stream, sourceFilePath, token );
      await WriteFileAsync(
        stream,
        BuildPath(
          destinationFolder,
          Path.GetFileName( sourceFilePath ) ),
        token );
      await DeleteFileAsync( sourceFilePath );

    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "MoveFileAsync error" );
      throw;
    }
  }

  /// <summary>
  /// Copies a file presented by stream to file store.
  /// </summary>
  /// <param name="stream">File stream.</param>
  /// <param name="filePath">Target folder name.</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>The file path.</returns>
  public override async Task<string> WriteFileAsync(
    Stream stream,
    string filePath,
    CancellationToken token = default)
  {
    Guard.Argument( stream ).NotNull( nameof( stream ) );
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      logger.LogInformation( $"WriteFileAsync: {_containerName} {filePath}" );

      (string container, string folder) = GetFilePath( filePath );
      if ( container != _containerName )
        throw new UnauthorizedAccessException( "Invalid container" );

      await _blobServiceClient
            .GetBlobContainerClient( _containerName )
            .GetBlobClient( folder )
            .UploadAsync( stream, overwrite: true, token );

      return filePath;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "WriteFileAsync Exception" );
      throw;
    }

  }

  /// <summary>
  /// Reads a file from storage into stream.
  /// </summary>
  /// <param name="stream">File stream.</param>
  /// <param name="filePath">Relative to root file path.</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>true if the file was read successfully; otherwise, false.</returns>
  public override async Task<bool> ReadFileAsync(
    Stream stream,
    string filePath,
    CancellationToken token = default)
  {
    Guard.Argument( stream ).NotNull( nameof( stream ) );
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      (string container, string folder) = GetFilePath( filePath );
      if ( container != _containerName )
        throw new UnauthorizedAccessException( "Invalid container" );

      await _blobServiceClient
           .GetBlobContainerClient( _containerName )
           .GetBlobClient( folder )
           .DownloadToAsync( stream );

      logger.LogInformation( $"ReadFileAsync: {_containerName} {filePath}. File size: {stream.Length}" );

      stream.Position = 0;
      return true;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "ReadFileAsync Exception" );
      throw;
    }

  }

  /// <summary>
  /// Deletes a file from blob storage.
  /// </summary>
  /// <param name="filePath">Relative to root file path.</param>
  /// <returns>true if the file was deleted successfully; otherwise, false.</returns>
  public override async Task<bool> DeleteFileAsync(
    string filePath)
  {
    Guard.Argument( filePath ).NotEmpty( nameof( filePath ) );

    try
    {
      var physicalFileName = filePath;
      logger.LogInformation( $"DeleteFileAsync '{physicalFileName}'" );

      await _blobServiceClient
        .GetBlobContainerClient( _containerName )
        .DeleteBlobAsync( physicalFileName );

      return true;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "DeleteFileAsync Exception" );
      throw;
    }

  }

  /// <summary>
  /// Deletes a folder from blob storage.
  /// </summary>
  /// <param name="folderName">Folder to delete.</param>
  public override async Task DeleteFolderAsync(
    string folderName)
  {
    Guard.Argument( folderName ).NotEmpty( nameof( folderName ) );

    await DeleteImportFilesAsync(
      _blobServiceClient.GetBlobContainerClient( _containerName ),
      GetPhysicalPath( folderName ),
      null );
  }

  /// <summary>
  /// Extracts a file to blob storage.
  /// </summary>
  /// <param name="archiveFileName">Source file name.</param>
  /// <param name="extractDirectory">Target extraction folder name.</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>The extraction directory.</returns>
  public override async Task<string> ExtractFileToStorageAsync(
    string archiveFileName,
    string extractDirectory,
    CancellationToken token = default)
  {
    Guard.Argument( archiveFileName ).NotEmpty( nameof( archiveFileName ) );
    Guard.Argument( extractDirectory ).NotEmpty( nameof( extractDirectory ) );

    try
    {
      logger.LogInformation( $"extracting {archiveFileName} -> {extractDirectory}" );

      using var stream = new MemoryStream();
      var fileProcessor = new ZipFileProcessor(
        _blobServiceClient.GetBlobContainerClient( _containerName ),
        logger,
        cfg );

      await ReadFileAsync(
        stream,
        archiveFileName,
        token );

      await fileProcessor.ProcessFileAsync(
        stream,
        extractDirectory,
        token );

      // TODO: correct this later
      return extractDirectory;
    }
    catch ( Exception ex )
    {
      logger.LogError( ex, "ExtractFileToStorageAsync Exception" );
      throw;
    }

  }

  /// <summary>
  /// Creates an archive file from a folder.
  /// </summary>
  /// <param name="archive">Archive file stream.</param>
  /// <param name="folderName">Source file folder name.</param>
  /// <param name="zipEntryFolderName">Zip entry folder name.</param>
  /// <param name="appendToStream">Append or replace stream contents.</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>true if the folder was copied to the archive successfully; otherwise, false.</returns>
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
      IList<BlobItem> blobs;

      var physicalFolder = GetPhysicalPath( folderName );
      logger.LogInformation( $"reading '{physicalFolder}' for files to add to stream" );

      blobs = _blobServiceClient
        .GetBlobContainerClient( _containerName )
        .GetBlobs( prefix: physicalFolder ).ToList();

      foreach ( var blob in blobs )
      {
        var blobStream = new MemoryStream();

        await _blobServiceClient
             .GetBlobContainerClient( _containerName )
             .GetBlobClient( blob.Name )
             .DownloadToAsync( blobStream );

        blobStream.Position = 0;

        var entryPath = BuildPath( zipEntryFolderName, Path.GetFileName( blob.Name ) );
        logger.LogInformation( $"  adding '{blob.Name}' to archive '{entryPath}'. size = {blobStream.Length}" );

        var entry = archive.CreateEntry( entryPath );
        using var entryStream = entry.Open();
        blobStream.CopyTo( entryStream );
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
  /// Gets files in a folder.
  /// </summary>
  /// <param name="folderName">Folder name.</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>A list of file names.</returns>
  public override IList<string> GetFiles(
    string folderName,
    CancellationToken token = default)
  {
    var fileNames = new List<string>();

    try
    {
      logger.LogInformation( $"looking in '{folderName}' for files" );

      var blobs = _blobServiceClient
        .GetBlobContainerClient( _containerName )
        .GetBlobs( prefix: folderName ).ToList();

      if ( blobs.Count == 0 )
        return fileNames;

      logger.LogInformation( $"  found '{blobs.Count}' files" );
      fileNames = blobs.Select( blob => Path.GetFileName( blob.Name ) ).ToList();

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

  /// <summary>
  /// Deletes import files asynchronously.
  /// </summary>
  /// <param name="containerClient">Blob container client.</param>
  /// <param name="prefix">Prefix for the files to delete.</param>
  /// <param name="segmentSize">Segment size for the deletion.</param>
  private async Task DeleteImportFilesAsync(
    BlobContainerClient containerClient,
    string prefix,
    int? segmentSize)
  {
    try
    {
      var zipFile = $"{prefix}.zip";

      // Call the listing operation and return pages of the specified size.
      var resultSegment = containerClient.GetBlobsByHierarchyAsync( prefix: prefix, delimiter: "/" )
          .AsPages( default, segmentSize );

      // Enumerate the blobs returned for each page.
      await foreach ( var blobPage in resultSegment )
      {
        // A hierarchical listing may return both virtual directories and blobs.
        foreach ( var blobhierarchyItem in blobPage.Values )
        {
          if ( blobhierarchyItem.IsPrefix )
          {
            // Call recursively with the prefix to traverse the virtual directory.
            await DeleteImportFilesAsync( containerClient, blobhierarchyItem.Prefix, null );
          }
          else
          {
            // don't delete the original import zip file
            if ( zipFile != blobhierarchyItem.Blob.Name )
            {
              logger.LogInformation( $" deleting existing: {blobhierarchyItem.Blob.Name}" );
              await containerClient.DeleteBlobAsync( blobhierarchyItem.Blob.Name );
            }
          }
        }

        Console.WriteLine();
      }
    }
    catch ( RequestFailedException e )
    {
      Console.WriteLine( e.Message );
      Console.ReadLine();
      throw;
    }
  }

  /// <summary>
  /// Gets the public URL for the file.
  /// </summary>
  /// <param name="path">The path.</param>
  /// <param name="fileName">The file name.</param>
  /// <returns>The public URL for the file.</returns>
  public override string GetUrlPath(string path, string fileName)
  {
    var physicalPath = BuildPath(
      cfg.GetAppSettings().FileStorageUrl,
      path,
      fileName );

    return physicalPath;
  }
}
