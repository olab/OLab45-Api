using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Endpoints;
using System.Net;

namespace OLab.Azure.Functions.ImportExport;

public class Import4Function : OLabFunction
{
  private readonly Import4Endpoint _endpoint;

  public Import4Function(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<Import4Function>( loggerFactory, true );

    _endpoint = new Import4Endpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Runs an import
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>IActionResult</returns>
  [Function( "Import4" )]
  public async Task<IActionResult> ImportAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "import4" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel)
  {
    try
    {
      Logger.LogDebug( $"ImportAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      if ( request.Body == null )
        throw new ArgumentNullException( nameof( request.Body ) );

      if ( !await auth.HasAccessAsync( IOLabAuthorization.AclBitMaskExecute, "Import", 0 ) )
        throw new OLabUnauthorizedException();

      var parser = await MultipartFormDataParser.ParseAsync( request.Body );
      if ( parser.Files.Count == 0 )
        throw new Exception( "No files were uploaded" );

      var stream = parser.Files[ 0 ].Data;

      Logger.LogInformation( $"Loading archive: '{parser.Files[ 0 ].FileName}'" );

      var mapPhys = await _endpoint.ImportAsync(
        auth,
        stream,
        parser.Files[ 0 ].FileName,
        cancel );

      var createdAt = mapPhys.CreatedAt.Value;
      createdAt = DateTime.SpecifyKind( createdAt, DateTimeKind.Utc );

      var dto = new ImportResponse
      {
        Id = mapPhys.Id,
        Name = mapPhys.Name,
        CreatedAt = mapPhys.CreatedAt.Value,
        LogMessages = Logger.GetMessages( OLabLogMessage.MessageLevel.Info ).Select( x => x.Message ).ToList()
      };

      return request
        .CreateResponse( OLabObjectResult<ImportResponse>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapGetShortStatusAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }
  }

  [Function( "Export4AsJson" )]
  public async Task<IActionResult> ExportAsJsonAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "import4/export/{id}/json" )] HttpRequestData request,
    FunctionContext hostContext,
    uint id,
    CancellationToken token)
  {
    try
    {
      Logger.LogDebug( $"ExportAsJsonAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      if ( !await auth.HasAccessAsync( IOLabAuthorization.AclBitMaskExecute, "Export", 0 ) )
        throw new OLabUnauthorizedException();

      var dto = await _endpoint.ExportAsync( id, token );
      return request
        .CreateResponse( OLabObjectResult<MapsFullRelationsDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapGetShortStatusAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

  [Function( "Export4" )]
  public async Task<IActionResult> ExportAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "import4/export/{id}" )] HttpRequestData request,
    FunctionContext hostContext,
    uint id,
    CancellationToken token)
  {
    try
    {
      Logger.LogDebug( $"Export" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      if ( !await auth.HasAccessAsync( IOLabAuthorization.AclBitMaskExecute, "Export", 0 ) )
        throw new OLabUnauthorizedException();

      using ( var memoryStream = new MemoryStream() )
      {
        await _endpoint.ExportAsync( memoryStream, id, token );

        memoryStream.Position = 0;
        var now = DateTime.UtcNow;

        var fileDownloadName = $"OLab4Export.map{id}.{now.ToString( "yyyyMMddHHmm" )}.zip";

        var result = new ObjectResult( memoryStream.ToArray().ToString() )
        {
          StatusCode = (int)HttpStatusCode.OK,
          ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection()
          {
            "application/zip"
          }
        };

        response.Headers.Add( "Content-Length", $"{memoryStream.Length}" );
        response.Headers.Add( "Content-Disposition", $"attachment; filename={fileDownloadName}; filename*=UTF-8'{fileDownloadName}" );

        return result;
      }
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "Export4" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }
}

