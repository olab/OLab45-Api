using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class Diagnostics
  {
    public Diagnostics()
    {
    }

    [JsonProperty("stack")]
    public string Stack { get; set; }
    [JsonProperty("file")]
    public string File { get; set; }
    [JsonProperty("line")]
    public string Line { get; set; }
  }

  public class OLabAPIResponse<D> : ActionResult
  {
    public const string MessageSuccess = "success";

    public OLabAPIResponse()
    {
      Diagnostics = new List<Diagnostics>();
      Message = MessageSuccess;
      ErrorCode = StatusCodes.Status200OK;
    }

    [JsonProperty("extended_status_code")]
    public int? Status { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("error_code")]
    public int? ErrorCode { get; set; }
    [JsonProperty("diagnostics")]
    public IList<Diagnostics> Diagnostics { get; set; }
    [JsonProperty("data")]
    public D Data { get; set; }
  }
}
