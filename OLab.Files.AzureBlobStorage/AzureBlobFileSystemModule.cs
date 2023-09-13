﻿using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class AzureBlobFileSystemModule : IFileStorageModule
  {
    private readonly IOLabLogger _logger;
    private readonly AppSettings _appSettings;

    public AzureBlobFileSystemModule(IOLabLogger logger, AppSettings appSettings)
    {
      _logger = logger;
      _appSettings = appSettings;
    }

    public void AttachUrls(IList<SystemFiles> items)
    {
      throw new NotImplementedException();
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
      throw new NotImplementedException();
    }

    public Task<string> UploadFile(Stream file, string fileName, CancellationToken token)
    {
      throw new NotImplementedException();
    }
  }
}