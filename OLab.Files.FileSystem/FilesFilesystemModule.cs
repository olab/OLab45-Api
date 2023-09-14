﻿using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System.Net.NetworkInformation;

namespace OLab.Files.FileSystem
{
  [OLabModule("FILESYSTEM")]
  public class FilesFilesystemModule : OLabModuleProvider<IFileStorageModule>
  {
    private readonly IOLabLogger _logger;

    public FilesFilesystemModule(IOLabLogger logger, IConfiguration configuration) : base(logger, configuration)
    {
      _logger = logger;
    }

    public void AttachUrls(IList<SystemFiles> items)
    {
      foreach (var item in items)
      {
        var scopeLevel = item.ImageableType;
        var scopeId = item.ImageableId;

        var subPath = GetBasePath(scopeLevel, scopeId, item.Path);
        var physicalPath = GetPhysicalPath(scopeLevel, scopeId, item.Path);

        if (File.Exists(physicalPath))
          item.OriginUrl = $"/{Path.GetFileName(_configuration.GetAppSettings().Value.FileStorageFolder)}/{subPath}";
        else
          item.OriginUrl = null;
      }
    }

    private string GetBasePath(string scopeLevel, uint scopeId, string filePath)
    {
      var subPath = $"{scopeLevel}/{scopeId}/{filePath}";
      return subPath;
    }

    private string GetPhysicalPath(string scopeLevel, uint scopeId, string filePath)
    {
      var subPath = $"{scopeLevel}/{scopeId}/{filePath}";
      var physicalPath = Path.Combine(
        _configuration.GetAppSettings().Value.FileStorageFolder, 
        subPath.Replace('/', Path.DirectorySeparatorChar));
      return physicalPath;
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
      Guard.Argument(sourcePath).NotEmpty(nameof(sourcePath));
      Guard.Argument(destinationPath).NotEmpty(nameof(destinationPath));

      File.Move(sourcePath, destinationPath);

      _logger.LogInformation($"moved file from '{sourcePath}' to {destinationPath}");
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

  }
}