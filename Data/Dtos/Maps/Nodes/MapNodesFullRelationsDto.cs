using System.Collections.Generic;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapsNodesFullRelationsDto : MapNodesFullDto
  {
    [JsonProperty("links")]
    public IList<MapNodeLinksDto> MapNodeLinks { get; set; }

    // [JsonProperty("counters")]
    // public IList<MapNodeCountersDto> MapNodeCounters { get; set; }

    public int LinkCount { get { return MapNodeLinks.Count; } }

  }
}
