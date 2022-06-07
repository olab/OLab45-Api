using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class DesignerConstantsDto
  {
    [JsonProperty("id")]
    public uint Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("scopeLevel")]
    public string ImageableType { get; set; }
    [JsonProperty("parentId")]
    public uint ImageableId { get; set; }    
    [JsonProperty("wiki")]
    public string Wiki { get; set; }    

    [JsonProperty("acl")]
    public string Acl { get; set; }    

    [JsonProperty("isSystem")]
    public int? IsSystem { get; set; }  
    [JsonProperty("url")]
    public int? Url { get; set; }  

  }
}