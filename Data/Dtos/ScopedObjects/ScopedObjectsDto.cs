using System.Collections.Generic;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class ScopedObjectsDto
  {
    public ScopedObjectsDto()
    {
      Questions = new List<QuestionsFullDto>();    
      Constants = new List<ConstantsDto>();    
      Counters = new List<CountersDto>();    
      Files = new List<FilesFullDto>();    
      Scripts = new List<ScriptsDto>();    
      Themes = new List<ThemesFullDto>();   
      CounterActions = new List<CounterActionsDto>();
    }

    [JsonProperty("questions")]
    public List<QuestionsFullDto> Questions { get; set; }
    [JsonProperty("constants")]
    public List<ConstantsDto> Constants { get; set; }
    [JsonProperty("counters")]
    public List<CountersDto> Counters { get; set; }
    [JsonProperty("files")]
    public List<FilesFullDto> Files { get; set; }
    [JsonProperty("scripts")]
    public List<ScriptsDto> Scripts { get; set; }
    [JsonProperty("themes")]
    public List<ThemesFullDto> Themes { get; set; }
    [JsonProperty("counteractions")]
    public List<CounterActionsDto> CounterActions { get; set; }    

  }
}
