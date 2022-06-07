using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class ScriptsFullDto : ScriptsDto
  {
    [JsonProperty("source")]
    public byte[] Source { get; set; }
    [JsonProperty("isRaw")]
    public ulong? IsRaw { get; set; }
    [JsonProperty("order")]
    public int? Order { get; set; }
    [JsonProperty("postLoadId")]
    public int? PostloadId { get; set; }
    [JsonProperty("scriptCol")]
    public string SystemScriptscol { get; set; }

    // calculated properties
    [JsonProperty("wiki")]
    public string Wiki { get; set; }     
  }
}