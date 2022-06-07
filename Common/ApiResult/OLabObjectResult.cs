using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;

namespace OLabWebAPI.Common
{
  public class OLabObjectResult<D>
  {
    public static JsonResult Result(D value)
    {
      var result = new OLabAPIResponse<D>
      {
        Data = value
      };

      return new JsonResult( result );
    }
  }
}