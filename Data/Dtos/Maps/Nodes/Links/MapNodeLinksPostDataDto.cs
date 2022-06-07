using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapNodeLinksPostDataDto
  {
    [JsonProperty("destinationId")]
    public uint DestinationId { get; set; }
  }
}