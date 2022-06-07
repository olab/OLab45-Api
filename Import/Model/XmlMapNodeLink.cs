using System;
using System.Collections.Generic;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{
  public class XmlMapNodeLinks : XmlImportArray<MapNodeLinks>
  {
  }

  public class XmlMapNodeLink
  {
    public int Id;
    public int MapId;
    public int NodeId1;
    public int NodeId2;
    public int ImageId;
    public string Text;
    public int Order;
    public int Probability;
    public int Hidden;
  }
}