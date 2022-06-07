using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace OLabWebAPI.Common
{
  public class OLabObjectListResult<D>
  {
    public static JsonResult Result(IList<D> value)
    {
      var result = new OLabAPIPagedResponse<D>
      {
        Data = value,
        Count = value.Count
      };

      return new JsonResult( result );
    }
  }
}