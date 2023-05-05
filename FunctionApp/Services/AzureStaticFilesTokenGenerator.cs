using System;
using System.Collections.Generic;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;

namespace OLab.FunctionApp.Api.Services
{
  public class AzureStorageBlobOptionsTokenGenerator
  {
    private readonly AppSettings _appSettings;
    IDictionary<string, string> connectionStringSettings = new Dictionary<string, string>();

    public AzureStorageBlobOptionsTokenGenerator(
        AppSettings appSettings
    )
    {
      _appSettings = appSettings;
      var splitted = _appSettings.StaticFilesConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (var nameValue in splitted)
      {
        var splittedNameValue = nameValue.Split(new char[] { '=' }, 2);
        connectionStringSettings.Add(splittedNameValue[0], splittedNameValue[1]);
      }      
    }

    private string GetAdHocBlobSasToken(string containerName, string blobName, DateTime expiry)
    {
      var sasBuilder = new BlobSasBuilder()
      {
        BlobContainerName = containerName,
        BlobName = blobName,
        Resource = "b",//Value b is for generating token for a Blob and c is for container
        StartsOn = DateTime.UtcNow.AddMinutes(-2),
        ExpiresOn = expiry,
      };

      sasBuilder.SetPermissions(BlobSasPermissions.Read); //multiple permissions can be added by using | symbol

      var sasToken = sasBuilder.ToSasQueryParameters(
        new StorageSharedKeyCredential(connectionStringSettings["AccountName"], connectionStringSettings["AccountKey"]));

      return $"{new BlobClient(_appSettings.StaticFilesConnectionString, containerName, blobName).Uri}?{sasToken}";
    }

    public string GenerateSasToken(
        string containerName,
        string blobName
    )
    {
      return GetAdHocBlobSasToken(
          containerName,
          blobName,
          DateTime.UtcNow.AddHours(1));
    }
  }
}

// https://www.craftedforeveryone.com/beginners-guide-and-reference-to-azure-blob-storage-sdk-v12-dot-net-csharp/#generate_adhoc_sas_token_for_a_blob
