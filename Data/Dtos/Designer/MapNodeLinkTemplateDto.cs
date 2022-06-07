using Newtonsoft.Json;

namespace OLabWebAPI.Dto.Designer
{
  public class MapNodeLinkTemplateDto
  {
    [JsonProperty("text")]
    public string Text { get; set; }
    [JsonProperty("hidden")]
    public int Hidden { get; set; }
    [JsonProperty("order")]
    public int Order { get; set; }
    [JsonProperty("probability")]
    public int Probability { get; set; }
    [JsonProperty("thicknees")]
    public int Thickness { get; set; }
    [JsonProperty("color")]
    public string Color { get; set; }
    [JsonProperty("lineStyleId")]
    public uint? LinkStyleId { get; set; }
    [JsonProperty("linetype")]
    public int LineType { get; set; }
    [JsonProperty("followOnce")]
    public int FollowOnce { get; set; }
  }
}
