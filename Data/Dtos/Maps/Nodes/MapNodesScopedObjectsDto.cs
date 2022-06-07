using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapNodesScopedObjectsDto : MapNodesDto
  {
    [JsonProperty("scopedObjects")]
    public ScopedObjectsDto ScopedObjects { get; set; }

  }
}
