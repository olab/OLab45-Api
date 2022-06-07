using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{

  public class XmlMapAvatarDto : XmlImportDto<XmlMapAvatars>
  {
    private readonly AvatarsMapper _avMapper;
    private readonly ObjectMapper.Files _fileMapper;

    public XmlMapAvatarDto(Importer importer) : base(importer, "map_avatar.xml")
    {
      _avMapper = new AvatarsMapper(GetLogger(), GetWikiProvider());
      _fileMapper = new ObjectMapper.Files(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.map_avatar.Elements();
    }

    public SystemFiles CreateAvatarSystemFile( IEnumerable<dynamic> elements, MapAvatars avatar )
    {
      var phys = new SystemFiles
      {
        Id = 0,
        Name = $"Avatar{avatar.Id}",
        Mime = "image/png",
        CreatedAt = DateTime.Now,
        Height = 300,
        HeightType = "px",
        Width = 300,
        WidthType = "px"
      };

      return phys;
    }

    /// <summary>
    /// Saves import object to database
    /// </summary>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      var avItem = _avMapper.ElementsToPhys(elements);
      var oldId = avItem.Id;

      avItem.Id = 0;

      var mapDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapDto) as XmlMapDto;
      avItem.MapId = mapDto.GetIdTranslation( GetFileName(), avItem.MapId).Value;

      Context.MapAvatars.Add(avItem);
      Context.SaveChanges();

      GetLogger().LogDebug($"Saved {GetFileName()} id {avItem.Id}");

      var fileItem = CreateAvatarSystemFile( elements, avItem );

      fileItem.ImageableId = avItem.MapId;
      fileItem.ImageableType = "Maps";
      fileItem.Path = avItem.Image;

      String publicFile = GetPublicFileDirectory(fileItem.ImageableType, (uint)fileItem.ImageableId, fileItem.Path);
      if (!File.Exists(publicFile))
        GetImporter().GetLogger().LogWarning(GetFileName(), 0, $"media file '{publicFile}' does not exist in public directory");

      Context.SystemFiles.Add(fileItem);
      Context.SaveChanges();

      CreateIdTranslation(oldId, fileItem.Id);
      GetLogger().LogDebug($"Saved SystemFile {fileItem.Id}");

      return true;
    }

  }

}