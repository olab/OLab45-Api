using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{

  public class XmlMetadataDto : XmlImportDto<XmlMetadata>
  {
    public XmlMetadataDto(Importer importer) : base(importer, "metadata.xml")
    {

    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.metadata.Elements();
    }    

    // there is no Save for MetaData records
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      return true;
    }    
  }
}