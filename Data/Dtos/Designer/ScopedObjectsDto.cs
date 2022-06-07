using System.Collections.Generic;
using Newtonsoft.Json;
using OLabWebAPI.Dto.Designer;

namespace OLabWebAPI.Dto.Designer
{
  public class ScopedObjectsDto
  {
    public ScopedObjectsDto()
    {
      Questions = new List<ScopedObjectDto>();    
      Constants = new List<ScopedObjectDto>();    
      Counters = new List<ScopedObjectDto>();    
      Files = new List<ScopedObjectDto>();    
      Scripts = new List<ScopedObjectDto>();    
    }

    [JsonProperty("questions")]
    public List<ScopedObjectDto> Questions { get; set; }
    [JsonProperty("constants")]
    public List<ScopedObjectDto> Constants { get; set; }
    [JsonProperty("counters")]
    public List<ScopedObjectDto> Counters { get; set; }
    [JsonProperty("files")]
    public List<ScopedObjectDto> Files { get; set; }
    [JsonProperty("scripts")]
    public List<ScopedObjectDto> Scripts { get; set; }

  }
}
