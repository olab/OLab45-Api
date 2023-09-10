using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

namespace OLab.Files.AzureBlobStorage
{
  [OLabModule("AZUREBLOBSTORAGE")]
  public class OLabFilesAzureBlobStorage : IFileStorageModule
  {
    private readonly OLabLogger _logger;

    public OLabFilesAzureBlobStorage(OLabLogger logger)
    {
      _logger = logger;
    }

    public void AttachUrls(AppSettings appSettings, IList<SystemFiles> items)
    {
      throw new NotImplementedException();
    }
  }
}