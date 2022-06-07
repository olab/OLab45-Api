
using System.Collections.Generic;

namespace OLabWebAPI.Importer
{
  public abstract class XmlImportObject
  {
  }

  public class XmlImportArray<T> : XmlImportObject
  {
    public IList<T> Data = new List<T>();
  }

}