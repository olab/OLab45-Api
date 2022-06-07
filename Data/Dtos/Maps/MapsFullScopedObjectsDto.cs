using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapsScopedObjectsDto : MapsDto
  {
    [JsonProperty("scopedObjects")]
    public ScopedObjectsDto ScopedObjects { get; set; }

  }
}
