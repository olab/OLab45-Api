using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;
using OLabWebAPI.ObjectMapper;

namespace OLabWebAPI.Importer
{

  public class XmlMapVpdDto : XmlImportDto<XmlMapVpds>
  {
    private readonly ObjectMapper.MapVpd _mapper;

    public XmlMapVpdDto(Importer importer) : base(importer, "map_vpd.xml")
    {
      _mapper = new ObjectMapper.MapVpd(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Loads the specific import file into a model object
    /// </summary>
    /// <param name="importDirectory">Directory where import file exists</param>
    /// <returns></returns>
    public override bool Load(string importDirectory)
    {
      bool rc = true;

      try
      {
        SetImportDirectory(importDirectory);

        string filePath = Path.Combine(GetImportPackageDirectory(), GetFileName());
        GetLogger().LogDebug($"Loading '{Path.GetFileName(filePath)}'");

        if (File.Exists(filePath))
          _phys = DynamicXml.Load(filePath);
        else
        {
          GetLogger().LogDebug($"File {filePath} does not exist");
          return false;
        }

        var outerElements = GetElements(GetXmlPhys());
        int record = 0;

        foreach (var innerElements in outerElements)
        {
          try
          {
            ++record;
            var elements = (IEnumerable<dynamic>)innerElements.Elements();
            xmlImportElementSets.Add(elements);

            var item = _mapper.ElementsToPhys(elements);

            var phys = new XmlMapVpd
            {
              Id = item.Id,
              MapId = item.MapId,
              VpdTypeId = item.VpdTypeId
            };

            GetModel().Data.Add(phys);
            record++;
          }
          catch (Exception ex)
          {
            GetLogger().LogError(ex, $"error loading '{GetFileName()}' record #{record}: {ex.Message}");
          }

        }

        GetLogger().LogDebug($"imported {xmlImportElementSets.Count()} {GetFileName()} objects");

      }
      catch (Exception ex)
      {
        GetLogger().LogError(ex, $"Load error: {ex.Message}");
        rc = false;
      }

      return rc;
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.map_vpd.Elements();
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

      Context.MapVpds.Add(item);
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);

      return true;
    }

  }
}