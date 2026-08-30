using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol;

using OLab.Azure.Extensions;
using OLab.Common.ApiResult;
using OLab.Common.Exceptions;
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

  //public static ContentResult CreateResponse<T>(
  //  this HttpRequestData request,
  //  OLabApiResult<T> apiResponse)
  //{
  //  var contractResolver = new DefaultContractResolver
  //  {
  //    NamingStrategy = new CamelCaseNamingStrategy()
  //  };

  //  var content = new ContentResult
  //  {
  //    StatusCode = (int)apiResponse.ErrorCode,
  //    ContentType = "application/json",
  //    Content = JsonConvert.SerializeObject( apiResponse, new JsonSerializerSettings
  //    {
  //      ContractResolver = contractResolver
  //    } )
  //  };

  //  return content;

  //}

  public static HttpResponseData CreateResponse<T>(
    this HttpRequestData request,
    OLabApiResult<T> apiResponse)
  {
    var contractResolver = new DefaultContractResolver
    {
      NamingStrategy = new CamelCaseNamingStrategy()
    };

    // Serialize your API response exactly as before
    var json = JsonConvert.SerializeObject( apiResponse, new JsonSerializerSettings
    {
      ContractResolver = contractResolver
    } );

    // Build proper HttpResponseData (isolated worker compatible)
    var response = request.CreateResponse( (HttpStatusCode)apiResponse.ErrorCode );
    response.Headers.Add( "Content-Type", "application/json" );
    response.WriteString( json );

    return response;
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