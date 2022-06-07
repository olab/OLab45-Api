using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class FilesFull : OLabMapper<SystemFiles, FilesFullDto>
  {

    public FilesFull(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public FilesFull(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

  }
}