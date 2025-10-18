using Dawn;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Endpoints;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
      Logger.LogInformation( $"ImportAsync" );

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
      return ProcessException( request, ex, nameof( ImportAsync ) );
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
      Logger.LogInformation( $"ExportAsJsonAsync" );

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
      return ProcessException( request, ex, nameof( ExportAsJsonAsync ) );
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
      Logger.LogInformation( $"Export4" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      if ( !await auth.HasAccessAsync( IOLabAuthorization.AclBitMaskExecute, "Export", 0 ) )
        throw new OLabUnauthorizedException();

      var now = DateTime.UtcNow;
      var fileDownloadName = $"OLab4Export.map{id}.{now.ToString( "yyyyMMddHHmm" )}.zip";

      using ( var zipFileStream = new MemoryStream() )
      {
        await _endpoint.ExportAsync( zipFileStream, id, token );

        zipFileStream.Position = 0; // Reset the position of the existing stream

        // Return the ZIP file as an IActionResult
        return new FileContentResult( zipFileStream.ToArray(), "application/zip" )
        {
          FileDownloadName = fileDownloadName
        };
      }

    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ExportAsync ) );
    }

  }
}

