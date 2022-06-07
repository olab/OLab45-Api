using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto.Designer
{
  public class ScopeLevelObj
  {
    [JsonProperty("id")]
    public uint Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
  }
}