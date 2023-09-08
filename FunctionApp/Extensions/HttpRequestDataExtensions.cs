using Azure.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace OLab.FunctionApp.Extensions;

public static class HttpRequestDataExtensions
{
  public static HttpResponseData CreateResponse<T>(this HttpRequestData request, OLabAPIResponse<T> apiResponse)
  {
    var response = request.CreateResponse(apiResponse.ErrorCode);
    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

    var json = JsonConvert.SerializeObject(apiResponse);
    response.WriteString(json);

    return response;
  }

  public static async Task<T> ParseBodyFromRequestAsync<T>([NotNull] this HttpRequestData request)
      where T : class
  {
    var (isSuccess, body, exception) = await request.TryReadBodyAsAsync<T>();
    if (!isSuccess)
      throw new OLabInvalidRequestException(exception);

    return body;
  }

  public static async Task<(bool IsSuccess, T Value, Exception Exception)> TryReadBodyAsAsync<T>(
      [NotNull] this HttpRequestData request)
      where T : class
  {
    try
    {
      using var reader = new StreamReader(request.Body);
      var json = await reader.ReadToEndAsync();
      var result = JsonConvert.DeserializeObject<T>(json);
      return (result != null, result, null);
    }
    catch (Exception e)
    {
      return (false, default, e);
    }
  }
}