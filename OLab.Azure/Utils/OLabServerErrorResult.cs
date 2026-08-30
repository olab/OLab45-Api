using OLab.Common.ApiResult;
using System;
using System.Collections.Generic;
using System.Net;

public class OLabServerErrorResult
{
  public static OLabApiResult<string> Result(string errorMessage, HttpStatusCode ErrorCode = HttpStatusCode.InternalServerError)
  {
    return new OLabApiResult<string>()
    {
      Data = errorMessage,
      ErrorCode = ErrorCode,
      Message = "failed"
    };
  }

  public static OLabApiResult<string> Result(Exception ex, HttpStatusCode ErrorCode = HttpStatusCode.InternalServerError)
  {
    var diags = new List<Diagnostics>();

    return new OLabApiResult<string>( ErrorCode )
    {
      Diagnostics = diags,
      Message = ex.Message
    };
  }
}
