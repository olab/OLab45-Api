using Newtonsoft.Json;

namespace OLabWebAPI.Dto.Designer
{
  public class MapNodeTemplateDto
  {
    [JsonProperty("title")]
    public string Title { get; set; }   
    [JsonProperty("text")]
    public string Text { get; set; }
    [JsonProperty("x")]
    public double X { get; set; }
    [JsonProperty("y")]
    public double Y { get; set; }
    [JsonProperty("locked")]
    public int Locked { get; set; }
    [JsonProperty("collapsed")]
    public int Collapsed { get; set; }
    [JsonProperty("height")]
    public int Height { get; set; }
    [JsonProperty("width")]
    public int Width { get; set; }
    [JsonProperty("probability")]
    public int Probability { get; set; }
    [JsonProperty("info")]
    public string Info { get; set; }
    [JsonProperty("annotation")]
    public string Annotation { get; set; }
    [JsonProperty("linkStyleId")]
    public uint LinkStyleId { get; set; }
    [JsonProperty("linkTypeId")]
    public uint LinkTypeId { get; set; }
    [JsonProperty("typeId")]
    public uint TypeId { get; set; }
    [JsonProperty("isPrivate")]
    public int IsPrivate { get; set; }
    [JsonProperty("color")]
    public string Rgb { get; set; }
    [JsonProperty("visitOnce")]
    public int VisitOnce { get; set; }
    [JsonProperty("priorityId")]
    public int PriorityId { get; set; }
    [JsonProperty("isEnd")]
    public int End { get; set; }
  }
}
