using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
  public class QuestionsDto : ScopedObjectDto
  {
    [JsonProperty("questionType")]
    public uint EntryTypeId { get; set; }
  }
}