using System.Collections.Generic;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class QuestionsFullDto : QuestionsDto
  {
    public QuestionsFullDto()
    {
      Responses = new List<QuestionResponsesDto>();
    }

    [JsonProperty("stem")]
    public string Stem { get; set; }
    [JsonProperty("width")]
    public int Width { get; set; }
    [JsonProperty("height")]
    public int Height { get; set; }
    [JsonProperty("settings")]
    public string Settings { get; set; }    
    [JsonProperty("feedback")]
    public string Feedback { get; set; }
    [JsonProperty("prompt")]
    public string Prompt { get; set; }
    [JsonProperty("showAnswer")]
    public bool? ShowAnswer { get; set; }
    [JsonProperty("showSubmit")]
    public sbyte ShowSubmit { get; set; }
    [JsonProperty("layoutType")]
    public int TypeDisplay { get; set; }
    [JsonProperty("isPrivate")]
    public int IsPrivate { get; set; }
    [JsonProperty("order")]
    public int? Order { get; set; }   
    [JsonProperty("responses")]
    public List<QuestionResponsesDto> Responses { get; set; }

    // calculated/transient state fields
    [JsonProperty("tryCount")]
    public int TryCount { get; set; }
    [JsonProperty("numTries")]
    public int NumTries { get; set; }    
    [JsonProperty("disabled")]
    public int Disabled { get; set; }

    // calculated properties
    [JsonProperty("wiki")]
    public string Wiki { get; set; }   
    [JsonProperty("value")]
    public string Value { get; set; }

  }
}