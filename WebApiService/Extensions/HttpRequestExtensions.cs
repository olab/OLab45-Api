using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OLab.Api.Common;
using System.Net;

namespace OLabWebAPI.Extensions;

public static class HttpRequestExtensions
{
  public static ContentResult CreateResponse<T>(
    this HttpRequest request,
    OLabAPIResponse<T> apiResponse)
  {
    var contractResolver = new DefaultContractResolver
    {
      NamingStrategy = new CamelCaseNamingStrategy()
    };

    var content = new ContentResult
    {
      StatusCode = (int)apiResponse.ErrorCode,
      ContentType = "application/json",
      Content = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings
      {
        ContractResolver = contractResolver
      })
    };

    return content;

  }

  public static ContentResult CreateNoContentResponse(
    this HttpRequest request)
  {
    var content = new ContentResult
    {
      StatusCode = (int)HttpStatusCode.NoContent,
      ContentType = "application/json"
    };

    return content;
  }
}
