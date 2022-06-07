using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapNodesDto
  {
    [JsonProperty("sessionId")]
    public string SessionId { get; set; }    
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("id")]
    public uint? Id { get; set; }
  }
}
