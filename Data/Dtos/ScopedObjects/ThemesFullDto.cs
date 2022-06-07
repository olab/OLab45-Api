using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class ThemesFullDto : ThemesDto
  {
    [JsonProperty("header")]
    public string HeaderText { get; set; }
    [JsonProperty("footer")]
    public string FooterText { get; set; }
  }
}