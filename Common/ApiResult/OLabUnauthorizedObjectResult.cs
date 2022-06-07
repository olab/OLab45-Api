using System;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class OLabUnauthorizedObjectResult<D>
  {
    public static UnauthorizedObjectResult Result(D value)
    {
      return new UnauthorizedObjectResult(new OLabAPIResponse<D>()
      {
        Data = value
      });
    }
  }
}