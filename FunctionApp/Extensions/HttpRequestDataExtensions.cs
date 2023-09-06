using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using OLab.Api.Common.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace OLab.FunctionApp.Extensions;

public static class HttpRequestDataExtensions
{
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