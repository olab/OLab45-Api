using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using NuGet.Protocol;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.FunctionApp.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Extensions;

public static class HttpRequestDataExtensions
{
  public static HttpContext AsHttpContext(this HttpRequestData req)
  {
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Method = req.Method;
    httpContext.Request.Path = PathString.FromUriComponent(req.Url);
    httpContext.Request.Host = HostString.FromUriComponent(req.Url);
    httpContext.Request.Scheme = req.Url.Scheme;
    httpContext.Request.Query = new QueryCollection(QueryHelpers.ParseQuery(req.Query.ToString()));
    foreach (var header in req.Headers)
      httpContext.Request.Headers[header.Key] = header.Value.ToArray();
    httpContext.Request.Body = req.Body;
    return httpContext;
  }

  /// <summary>
  /// Create an HttpResponseData object from an exception
  /// </summary>
  /// <param name="request">HttpRequestData object</param>
  /// <param name="ex">Caught exception</param>
  /// <returns>HttpResponseData</returns>
  public static HttpResponseData CreateResponse(this HttpRequestData request, Exception ex)
  {
    HttpResponseData response = null;

    try
    {
      if (ex is OLabObjectNotFoundException)
        response = request.CreateResponse(OLabNotFoundResult<string>.Result(ex.Message));

      else if (ex is OLabUnauthorizedException)
        response = request.CreateResponse(
          OLabUnauthorizedObjectResult.Result(ex.Message));
      else
        response = request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }
    catch (Exception)
    {
      // eat all exceptions
    }

    return response;
  }

  /// <summary>
  /// Create an HttpResponseData object from a StatusCodeResult
  /// </summary>
  /// <param name="request">HttpRequestData object</param>
  /// <param name="statusCodeResult"></param>
  /// <returns>HttpResponseData</returns>
  public static HttpResponseData CreateResponse(this HttpRequestData request, StatusCodeResult statusCodeResult)
  {
    var response = request.CreateResponse((HttpStatusCode)statusCodeResult.StatusCode);

    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

    var json = JsonConvert.SerializeObject(statusCodeResult.ToJson());
    response.WriteString(json);

    return response;
  }

  public static HttpResponseData CreateResponse<T>(
    this HttpRequestData request,
    ObjectResult objectResult)
  {
    var olabResponse = objectResult as OLabAPIResponse<T>;

    var response = request.CreateResponse((HttpStatusCode)olabResponse.Status);

    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

    var json = JsonConvert.SerializeObject(objectResult.ToJson());
    response.WriteString(json);

    return response;
  }

  public static HttpResponseData CreateResponse<T>(
    this HttpRequestData request,
    OLabAPIResponse<T> apiResponse)
  {
    var response = request.CreateResponse(apiResponse.ErrorCode);
    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

    var json = JsonConvert.SerializeObject(apiResponse);
    response.WriteString(json);

    return response;
  }

  public static async Task<T> ParseBodyFromRequestAsync<T>(
    [NotNull] this HttpRequestData request)
      where T : class
  {
    var (isSuccess, body, exception) = await request.TryReadBodyAsAsync<T>();
    if (!isSuccess)
      throw new OLabInvalidRequestException(exception);

    return body;
  }

  public static async Task<(
    bool IsSuccess,
    T Value,
    Exception Exception)> TryReadBodyAsAsync<T>(
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