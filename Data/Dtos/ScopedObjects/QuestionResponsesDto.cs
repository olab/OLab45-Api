using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class QuestionResponsesDto : ScopedObjectDto
  {
    protected readonly QuestionsDto ParentQuestion;

    [JsonProperty("response")]
    public string Response { get; set; }
    [JsonProperty("feedback")]
    public string Feedback { get; set; }
    [JsonProperty("isCorrect")]
    public int IsCorrect { get; set; }
    [JsonProperty("score")]
    public int? Score { get; set; }
    [JsonProperty("from")]
    public string From { get; set; }
    [JsonProperty("to")]
    public string To { get; set; }
    [JsonProperty("order")]
    public uint Order { get; set; }
    [JsonProperty("questionId")]
    public uint QuestionId { get; set; }
    
    // calculated properties
    [JsonProperty("value")]
    public object Value { get; set; }
  }
}