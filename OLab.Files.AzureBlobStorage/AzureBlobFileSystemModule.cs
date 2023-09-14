﻿using Microsoft.Extensions.Configuration;
using OLab.Api.Model;
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
    private readonly IConfiguration _configuration;

    public AzureBlobFileSystemModule(IOLabLogger logger, IConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
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