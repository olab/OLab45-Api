using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class MapsDto
  {
    [JsonProperty("id")]
    public uint? Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    //[JsonProperty("userstate")]
    //public UserState UserState { get; set; }
  }
}
