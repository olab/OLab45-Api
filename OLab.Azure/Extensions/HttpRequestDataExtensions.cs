using Azure;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OLab.Azure.Extensions;

public static class HttpRequestDataExtensions
{
  public static HttpContext AsHttpContext(this HttpRequestData req)
  {
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Method = req.Method;
    httpContext.Request.Path = PathString.FromUriComponent( req.Url );
    httpContext.Request.Host = HostString.FromUriComponent( req.Url );
    httpContext.Request.Scheme = req.Url.Scheme;
    httpContext.Request.Query = new QueryCollection( QueryHelpers.ParseQuery( req.Query.ToString() ) );
    foreach ( var header in req.Headers )
      httpContext.Request.Headers[ header.Key ] = header.Value.ToArray();
    httpContext.Request.Body = req.Body;
    return httpContext;
  }

  /// <summary>
  /// Create an HttpResponseData object from a StatusCodeResult
  /// </summary>
  /// <param name="request">HttpRequestData object</param>
  /// <param name="statusCodeResult"></param>
  /// <returns>HttpResponseData</returns>
  public static HttpResponseData CreateResponse(this HttpRequestData request, StatusCodeResult statusCodeResult)
  {
    var response = request.CreateResponse( (HttpStatusCode)statusCodeResult.StatusCode );

    response.Headers.Add( "Content-Type", "application/json; charset=utf-8" );

    var json = JsonConvert.SerializeObject( statusCodeResult.ToJson() );
    response.WriteString( json );

    return response;
  }

  /// <summary>
  /// Create an HttpResponseData object from a StatusCodeResult
  /// </summary>
  /// <param name="request">HttpRequestData object</param>
  /// <param name="statusCodeResult"></param>
  /// <returns>HttpResponseData</returns>
  public static HttpResponseData CreateNoContentResponse(this HttpRequestData request)
  {
    var response = request.CreateResponse( HttpStatusCode.NoContent );
    return response;
  }

  public static ContentResult CreateResponse<T>(
    this HttpRequestData request,
    OLabApiResult<T> apiResponse)
  {
    var contractResolver = new DefaultContractResolver
    {
      NamingStrategy = new CamelCaseNamingStrategy()
    };

    var content = new ContentResult
    {
      StatusCode = (int)apiResponse.ErrorCode,
      ContentType = "application/json",
      Content = JsonConvert.SerializeObject( apiResponse, new JsonSerializerSettings
      {
        ContractResolver = contractResolver
      } )
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

  public static async Task<T> ParseBodyFromRequestAsync<T>(
    [NotNull] this HttpRequestData request, IOLabLogger logger)
      where T : class
  {
    try
    {
      string jsonString = await new StreamReader( request.Body ).ReadToEndAsync();
      logger.LogInformation( $"Request Body: {jsonString}" );

      var body = JsonConvert.DeserializeObject<T>( jsonString );
      return body;
    }
    catch ( Exception ex )
    {
      throw new OLabInvalidRequestException( ex );
    }
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
      using var reader = new StreamReader( request.Body );
      var json = await reader.ReadToEndAsync();
      var result = JsonConvert.DeserializeObject<T>( json );
      return (result != null, result, null);
    }
    catch ( Exception e )
    {
      return (false, default, e);
    }
  }
}