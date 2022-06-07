using System;
using System.Collections.Generic;

namespace OLabWebAPI.Importer
{
  public class XmlMapNodeSections : XmlImportArray<XmlMapNodeSection>
  {
  }

  public class XmlMapNodeSection
  {
    public int Id;
    public string Name;
    public int MapId;
    public string OrderBy;
  }

}