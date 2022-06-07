using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapNodeLinksPostResponseDto
  {
    [JsonProperty("id")]
    public uint Id { get; set; }
  }
}
