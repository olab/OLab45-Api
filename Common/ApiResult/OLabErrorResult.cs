using System;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class OLabErrorResult
  {
    public static BadRequestObjectResult Result(string errorMessage)
    {
      return new BadRequestObjectResult(new OLabAPIResponse<string>()
      {
        Data = errorMessage,
        ErrorCode = 400
      });
    }

  }
}