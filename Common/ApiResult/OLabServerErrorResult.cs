using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class OLabServerErrorResult
  {
    public static BadRequestObjectResult Result(string errorMessage)
    {
      return new BadRequestObjectResult(new OLabAPIResponse<string>()
      {
        Data = errorMessage,
        ErrorCode = 500,
        Message = "failed"
      });
    }

  }
}