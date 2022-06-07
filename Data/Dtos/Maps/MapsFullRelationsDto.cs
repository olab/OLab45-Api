using System.Collections.Generic;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapsFullRelationsDto
  {
    [JsonProperty("mapDetails")]
    public MapsFullDto Map { get; set; }
    [JsonProperty("nodes")]
    public IList<MapNodesFullDto> MapNodes { get; set; }
    [JsonProperty("edges")]
    public IList<MapNodeLinksDto> MapNodeLinks { get; set; }
    [JsonProperty("scopedObjects")]
    public ScopedObjectsDto ScopedObjects { get; set; }
    [JsonProperty("counterActions")]
    public IList<CounterActionsDto> CounterActions { get; set; }

    // calculated
    public int NodeCount { get; set; }

    public MapsFullRelationsDto()
    {
      MapNodes = new List<MapNodesFullDto>();
      MapNodeLinks = new List<MapNodeLinksDto>();
      ScopedObjects = new ScopedObjectsDto();
      CounterActions = new List<CounterActionsDto>();
    }

  }
}
