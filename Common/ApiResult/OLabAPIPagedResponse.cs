using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace OLabWebAPI.Common
{
  public class OLabAPIPagedResponse<D> : OLabAPIResponse<IList<D>>
  {
    [JsonProperty("count")]
    public int Count { get; set; }    
    [JsonProperty("remaining")]
    public int Remaining { get; set; }
  }
}
