using System;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Common
{
  public class OLabCreatedObjectResult<D>
  {
    public static CreatedAtActionResult Result(D value, uint id)
    {
      return new CreatedAtActionResult("created", "", new { id }, value );
    }

  }
}