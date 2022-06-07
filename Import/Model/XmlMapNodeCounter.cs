using System;
using System.Collections.Generic;

namespace OLabWebAPI.Importer
{
  public class XmlMapNodeCounters : XmlImportArray<XmlMapNodeCounter>
  {
  }

  public class XmlMapNodeCounter
  {
    public uint Id;
    public int NodeId;
    public int CounterId;
    public object Function;
    public int Display;
  }
}