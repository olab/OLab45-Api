using System;
using Microsoft.AspNetCore.Mvc;

public class OLabActionException : Exception
{
  public IActionResult Result;

  public OLabActionException(IActionResult result)
  {
    Result = result;
  }
}