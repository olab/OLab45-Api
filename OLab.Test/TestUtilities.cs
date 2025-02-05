using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Test;
internal static class TestUtilities
{
  public static IQueryable<T> LoadRecordsFromJson<T>(string filePath)
  {
    var json = File.ReadAllText( filePath );
    var data = JsonConvert.DeserializeObject<List<T>>( json );
    return data?.AsQueryable() ?? Enumerable.Empty<T>().AsQueryable();
  }
}
