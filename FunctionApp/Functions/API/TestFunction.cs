using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Model;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System;
using System.IO;
using Humanizer;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.FunctionApp.Extensions;
using OLab.Common.Contracts;

namespace OLab.FunctionApp.Functions.API;

public class TestFunction : OLabFunction
{
  public TestFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Logger = OLabLogger.CreateNew<TestFunction>(loggerFactory);
  }

  [Function("Bootstrap")]
  public HttpResponseData RunBootstrap(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
  {
    var maps = DbContext.Maps.FirstOrDefault(x => x.Id == 0);

    var response = request.CreateResponse(HttpStatusCode.OK);
    return response;
  }

  [Function("Health")]
  public HttpResponseData RunHealth(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request,
    FunctionContext hostContext)
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
      //expando.TryAdd(fileName.ToUpper(), assemblyDef.Version);
      modules.TryAdd(fileName.ToLower(), assemblyDef.Version.ToString());
    }

    var dto = new HealthResult
    {
      statusCode = HttpStatusCode.OK,
      main = mainAssemblyDef.Version,
      modules = modules,
      message = "Hello there!"
    };

    response = request.CreateResponse(OLabObjectResult<HealthResult>.Result(dto));
    return response;
  }

}
