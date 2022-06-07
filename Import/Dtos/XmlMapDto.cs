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

  public class XmlMapDto : XmlImportDto<XmlMap>
  {
    private readonly MapsMapper _mapper;

    public XmlMapDto(Importer importer) : base(importer, "map.xml")
    {
      _mapper = new MapsMapper(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Loads the specific import file into a model object
    /// </summary>
    /// <param name="importDirectory">Directory where import file exists</param>
    /// <returns></returns>
    // public override bool Load(string importDirectory)
    // {
    //   var result = base.Load(importDirectory);
    //   var elements = GetElements(GetXmlPhys());
    //   xmlImportElementSets.Add(elements);

    //   return result;
    // }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.Elements();
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
      var oldId = item.Id;
      item.Id = 0;

      Context.Maps.Add(item);
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);
      GetLogger().LogDebug($"Saved {GetFileName()} id {oldId} -> {item.Id}");

      GetModel().Data.Add(item);

      return true;
    }

  }
}