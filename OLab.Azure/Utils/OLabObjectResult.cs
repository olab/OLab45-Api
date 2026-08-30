using OLab.Common.ApiResult;
using System.Net;

namespace OLab.Azure.Utils;

public class OLabObjectResult<D>
{
  public OLabObjectResult(object value, HttpStatusCode status = HttpStatusCode.OK) 
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
