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

  public class XmlMapElementDto : XmlImportDto<XmlMapElements>
  {
    private readonly ObjectMapper.Files _mapper;

    public XmlMapElementDto(Importer importer) : base(importer, "map_element.xml")
    {
      _mapper = new ObjectMapper.Files(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.map_element.Elements();
    }

    /// Saves import object to database
    /// </summary>
    /// <param name="dtos">All import dtos (for lookups into related objects)</param>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      var item = _mapper.ElementsToPhys(elements);
      var oldId = item.Id;

      item.Id = 0;

      var mapDto = GetImporter().GetDto( Importer.DtoTypes.XmlMapDto ) as XmlMapDto;
      item.ImageableId = mapDto.GetIdTranslation( GetFileName(), item.ImageableId ).Value;
      item.ImageableType = "Maps";

      String path = Path.Combine(GetImportPackageDirectory(), "media", Path.GetFileName(item.Path));
      if (!File.Exists(path))
        GetImporter().GetLogger().LogWarning(GetFileName(), 0, $"media file '{Path.GetFileName(item.Path)}' does not exist in import package");
        
      item.Path = Path.GetFileName(item.Path);

      Context.SystemFiles.Add(item);
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);
      GetLogger().LogDebug($"Saved {GetFileName()} id {oldId} -> {item.Id}");

      return true;
    }
  }

}