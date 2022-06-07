using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto.Designer
{
  public class ScopedObjectDto
  {
    [JsonProperty("id")]
    public uint Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("scopeLevel")]
    public string ScopeLevel { get; set; }
    [JsonProperty("parentId")]
    public uint ParentId { get; set; }
    [JsonProperty("wiki")]
    public string Wiki { get; set; }
    [JsonProperty("acl")]
    public string Acl { get; set; }
    [JsonProperty("isSystem")]
    public int? IsSystem { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }

    public ScopedObjectDto()
    {
      Acl = "R";
      IsSystem = 0;
    }
  }
}