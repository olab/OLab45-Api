using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace OLab.Azure.Utils;

public class Diagnostics
{
  [JsonProperty( "stack" )]
  public string Stack { get; set; }

  [JsonProperty( "file" )]
  public string File { get; set; }

  [JsonProperty( "line" )]
  public int Line { get; set; }
}

public class OLabApiResult<D>
{
  public const string MessageSuccess = "success";

  public OLabApiResult(HttpStatusCode status = HttpStatusCode.OK)
  {
    Message = MessageSuccess;
    ErrorCode = status;
    Status = (int)status;
  }

  [JsonProperty( "extended_status_code" )]
  public int? Status { get; set; }

  [JsonProperty( "message" )]
  public string Message { get; set; }

  [JsonProperty( "error_code" )]
  public HttpStatusCode ErrorCode { get; set; }

  [JsonProperty( "diagnostics" )]
  public IList<Diagnostics> Diagnostics { get; set; } = new List<Diagnostics>();

  [JsonProperty( "data" )]
  public D Data { get; set; }
}
