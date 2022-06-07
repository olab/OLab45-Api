using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class LinksDto
  {    
    [JsonProperty("id")]
    public uint Id { get; set; }   
  }

  public class MapNodesPostResponseDto
  {
    public MapNodesPostResponseDto()
    {
      Links = new LinksDto();
    }

    [JsonProperty("id")]
    public uint Id { get; set; }
    [JsonProperty("links")]
    public LinksDto Links { get; set; }

  }
}
