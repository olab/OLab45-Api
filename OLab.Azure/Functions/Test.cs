using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace OLab.Azure.Functions
{
  public class TestFunction : OLabFunction
  {
    public TestFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext) : base( configuration, dbContext )
    {
      Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
      Logger = OLabLogger.CreateNew<TestFunction>( loggerFactory );
    }

    [Function( "Bootstrap" )]
    public IActionResult Bootstrap(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get" )] HttpRequestData request)
    {
      var mapCount = DbContext.Maps.Count( x => x.Id > 0 );
      Logger.LogInformation( $"Found {mapCount} maps." );

      var response = new StringMessageResponse { Message = $"Bootstrap found {mapCount} maps." };
      return new OLabObjectResult<StringMessageResponse>( response );
    }

    [Function( "HealthCheck" )]
    public IActionResult HealthCheck(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get" )] HttpRequest req)
    {
      Logger.LogDebug( "Test debug message." );
      Logger.LogError( "Test error message" );
      Logger.LogFatal( "Test fatal Message" );
      Logger.LogInformation( "Test info Message" );

      Logger.LogInformation( "C# HTTP trigger function processed a request." );
      var response = new StringMessageResponse { Message = $"Welcome to Azure Functions." };
      return new OLabObjectResult<StringMessageResponse>( response );
    }

    [Function( "Modules" )]
    public IActionResult Modules(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get" )] HttpRequest req)
    {
      var asms = AppDomain.CurrentDomain.GetAssemblies().ToList();
      var olabAsms = asms.Where( x => x.FullName.ToLower().Contains( "olab" ) );

      var modules = new Dictionary<string, string>();

      var assembly = Assembly.GetEntryAssembly(); // Assembly.GetExecutingAssembly();
      var exeFvi = FileVersionInfo.GetVersionInfo( assembly.Location );
      var exeFileName = Path.GetFileNameWithoutExtension( exeFvi.FileName );

      var mainMetadata = AssemblyMetadata.CreateFromFile( exeFvi.FileName );
      var mainModule = mainMetadata.GetModules().First();
      var mainReader = mainModule.GetMetadataReader();
      var mainAssemblyDef = mainReader.GetAssemblyDefinition();

      foreach ( var olabAsm in olabAsms )
      {
        var fvi = FileVersionInfo.GetVersionInfo( olabAsm.Location );
        var fileName = Path.GetFileName( fvi.FileName );

        var metadata = AssemblyMetadata.CreateFromFile( fvi.FileName );
        var module = metadata.GetModules().First();
        var reader = module.GetMetadataReader();
        var assemblyDef = reader.GetAssemblyDefinition();

        Logger.LogInformation( $"  {fileName} {assemblyDef.Version}" );
        modules.TryAdd( fileName, assemblyDef.Version.ToString() );
      }

      var response = new HealthResult
      {
        StatusCode = HttpStatusCode.OK,
        Main = mainAssemblyDef.Version,
        Modules = modules
      };

      Logger.LogInformation( $"  {JsonConvert.SerializeObject(response)}" );

      return new OLabObjectResult<HealthResult>( response );
    }
  }
}
