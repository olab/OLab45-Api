using System;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class OLabUnauthorizedResult
  {
    public static UnauthorizedResult Result()
    {
      return new UnauthorizedResult();
    }
  }
}