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

  public class XmlMapCounterDto : XmlImportDto<XmlMapCounters>
  {
    private readonly ObjectMapper.Counters _mapper;

    public XmlMapCounterDto(Importer importer) : base(importer, "map_counter.xml")
    {
      _mapper = new ObjectMapper.Counters(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)GetXmlPhys().map_counter.Elements();
    }

    /// <summary>
    /// Saves import object to database
    /// </summary>
    /// <param name="dtos">All import dtos (for lookups into related objects)</param>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      var item = _mapper.ElementsToPhys(elements);
      item.ImageableId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "map_id").Value);
            
      var oldId = item.Id;

      item.Id = 0;      

      var mapDto = GetImporter().GetDto( Importer.DtoTypes.XmlMapDto ) as XmlMapDto;
      item.ImageableId = mapDto.GetIdTranslation( GetFileName(), item.ImageableId ).Value;
      item.ImageableType = "Maps";

      Context.SystemCounters.Add(item);
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);
      GetLogger().LogDebug($"Saved {GetFileName()} id {oldId} -> {item.Id}");

      return true;
    }  

  }
}