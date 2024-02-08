using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLabWebAPI.Endpoints.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OLabWebAPI.Controllers;

[Route("olab/api/v3/[action]")]
[ApiController]
public class TestController : Controller
{
  protected IOLabLogger Logger = null;

  public TestController(
    ILoggerFactory loggerFactory
  )
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<AuthController>(loggerFactory);
  }

  public IActionResult Index()
  {
    return View();
  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Health()
  {
    var asms = AppDomain.CurrentDomain.GetAssemblies().ToList();
    var olabAsms = asms.Where(x => x.FullName.ToLower().Contains("olab"));

    var expando = new ExpandoObject() as IDictionary<string, Object>;
    // x.Add("NewProp", string.Empty);

    var assembly = Assembly.GetEntryAssembly(); // Assembly.GetExecutingAssembly();
    var exeFvi = FileVersionInfo.GetVersionInfo(assembly.Location);
    var exeFileName = Path.GetFileNameWithoutExtension(exeFvi.FileName);

    foreach (var olabAsm in olabAsms)
    {
      var fvi = FileVersionInfo.GetVersionInfo(olabAsm.Location);
      var fileName = Path.GetFileName(fvi.FileName);

      Logger.LogInformation($"  {fileName} {fvi.FileVersion}");
      expando.TryAdd(fileName, fvi.FileVersion);
    }

    return Ok(new
    {
      statusCode = 200,
      main = exeFvi.FileVersion,
      modules = expando,
      message = "Hello there!"
    });
  }
}
