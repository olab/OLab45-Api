using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Common;

namespace OLabWebAPI.Dto
{
  public class OlabResult : ActionResult
  {
    public const string MessageSuccess = "success";

    public OlabResult()
    {
      Diagnostics = new List<Diagnostics>();
      Message = MessageSuccess;
      ErrorCode = StatusCodes.Status200OK;
    }

    [JsonProperty("status")]
    public int Status { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("error_code")]
    public int ErrorCode { get; set; }
    [JsonProperty("diagnostics")]
    public IList<Diagnostics> Diagnostics { get; set; }
  }
}
