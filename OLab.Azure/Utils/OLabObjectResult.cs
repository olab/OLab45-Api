using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using System.Net;

namespace OLab.Azure.Utils;

public class OLabObjectResult<D> : ObjectResult
{
  public OLabObjectResult(object value, HttpStatusCode status = HttpStatusCode.OK) : base( value )
  {
  }

  public static OLabApiResult<D> Result(D value, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    var result = new OLabApiResult<D>
    {
      Data = value,
      ErrorCode = statusCode
    };

    return result;
  }
}
