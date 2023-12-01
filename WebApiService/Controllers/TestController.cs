using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OLabWebAPI.Controllers;

[Route("olab/api/v3/[action]")]
[ApiController]
public class TestController : Controller
{
  public IActionResult Index()
  {
    return View();
  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Health()
  {
    return Ok(new { statusCode = 200, message = "Hello there!" });
  }
}
