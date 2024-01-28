using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OLab.Api.Common;

namespace OLabWebAPI.Extensions;

public static class HttpRequestExtensions
{
  public static ContentResult CreateResponse<T>(
    this HttpRequest request,
    OLabAPIResponse<T> apiResponse)
  {
    var content = new ContentResult
    {
      StatusCode = (int)apiResponse.ErrorCode,
      ContentType = "application/json",
      Content = JsonConvert.SerializeObject(apiResponse)
    };

    return content;
  }
}
