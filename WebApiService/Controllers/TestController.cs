using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Utils;
using OLab.Common.Contracts;
using OLab.Common.Interfaces;
using OLabWebAPI.Endpoints.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

    var modules = new Dictionary<string, string>();

    var assembly = Assembly.GetEntryAssembly(); // Assembly.GetExecutingAssembly();
    var exeFvi = FileVersionInfo.GetVersionInfo(assembly.Location);
    var exeFileName = Path.GetFileNameWithoutExtension(exeFvi.FileName);

    var mainMetadata = AssemblyMetadata.CreateFromFile(exeFvi.FileName);
    var mainModule = mainMetadata.GetModules().First();
    var mainReader = mainModule.GetMetadataReader();
    var mainAssemblyDef = mainReader.GetAssemblyDefinition();

    foreach (var olabAsm in olabAsms)
    {
      var fvi = FileVersionInfo.GetVersionInfo(olabAsm.Location);
      var fileName = Path.GetFileName(fvi.FileName);

      var metadata = AssemblyMetadata.CreateFromFile(fvi.FileName);
      var module = metadata.GetModules().First();
      var reader = module.GetMetadataReader();
      var assemblyDef = reader.GetAssemblyDefinition();

      Logger.LogInformation($"  {fileName} {assemblyDef.Version}");
      modules.TryAdd(fileName, assemblyDef.Version.ToString());
    }

    var dto = new HealthResult
    {
      statusCode = HttpStatusCode.OK,
      main = mainAssemblyDef.Version,
      modules = modules,
      message = "Hello there!"
    };

    return Ok(OLabObjectResult<HealthResult>.Result(dto));
  }
}
