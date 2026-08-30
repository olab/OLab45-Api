using DocumentFormat.OpenXml.Drawing;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace OLab.Azure.Utils;

internal static class OLabFunctionResponses
{
  public static HttpResponseData OLabNotFoundResponse(HttpRequestData request)
  {
    var response = request.CreateResponse( HttpStatusCode.NotFound );
    response.Headers.Add( "Content-Type", "application/json" );
    response.WriteString( "{\"message\":\"not found\"}" );
    return response;
  }

  public static HttpResponseData OLabUnauthorizedResponse(HttpRequestData request)
  {
    var response = request.CreateResponse( HttpStatusCode.Unauthorized );
    response.Headers.Add( "Content-Type", "application/json" );
    response.WriteString( "{\"message\":\"unauthorized\"}" );
    return response;
  }

  public static HttpResponseData OLabFileContentResponse(HttpRequestData request, string fileDownloadName)
  {
    // Build proper HttpResponseData for file download
    var response = request.CreateResponse( HttpStatusCode.OK );

    response.Headers.Add( "Content-Type", "application/zip" );
    response.Headers.Add(
        "Content-Disposition",
        $"attachment; filename=\"{fileDownloadName}\""
    );
    return response;
  }

  public static HttpResponseData OLabNoContentResponse(HttpRequestData request)
  {
    // Isolated-worker equivalent of NoContentResult
    return request.CreateResponse( HttpStatusCode.NoContent );
  }

  public static HttpResponseData OLabOkStringResponse(HttpRequestData request, string payload)
  {
    var response = request.CreateResponse( HttpStatusCode.OK );
    response.Headers.Add( "Content-Type", "text/plain; charset=utf-8" );
    return response;
  }
}
