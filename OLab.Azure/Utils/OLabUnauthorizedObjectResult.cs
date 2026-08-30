using OLab.Common.ApiResult;
using System.Net;

namespace OLab.Azure.Utils;

public class OLabUnauthorizedObjectResult
{
  public static OLabApiResult<string> Result(string value)
  {
    var apiResponse = new OLabApiResult<string>()
    {
      Data = value,
      ErrorCode = HttpStatusCode.Unauthorized,
      Status = (int)HttpStatusCode.Unauthorized,
      Message = "Unauthorized"
    };

    return apiResponse;
  }
}
