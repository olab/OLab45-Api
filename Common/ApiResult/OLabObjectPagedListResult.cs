using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace OLabWebAPI.Common
{
  public class OLabObjectPagedListResult<D>
  {
    public static JsonResult Result(IList<D> value, int remaining)
    {
      var result = new OLabAPIPagedResponse<D>
      {
        Data = value,
        Remaining = remaining,
        Count = value.Count
      };

      return new JsonResult( result );
    }
  }
}