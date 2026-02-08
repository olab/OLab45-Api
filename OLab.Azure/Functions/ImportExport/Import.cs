using Dawn;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Endpoints;
using OLab.Import;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ImportExport;

public class Import : OLabFunction
{
  private readonly Import4Endpoint _endpoint4;
  private readonly Import3Endpoint _endpoint3;

  public Import(ILoggerFactory loggerFactory,
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

    _endpoint4 = new Import4Endpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );

    _endpoint3 = new Import3Endpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  [Function( "Import" )]
  public async Task<IActionResult> ImportAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "import" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancel)
  {
    try
    {
      Logger.LogInformation( $"ImportAsync" );

      Api.Model.Maps mapPhys = null;

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

      // get list of archive files to determine if OLab3/4 import
      var files = ZipFileHelper.GetFiles( stream );

      if ( files.Contains( "map.json" ) )
      {
        Logger.LogInformation( "Detected OLab4 import file" );

        mapPhys = await _endpoint4.ImportAsync(
          auth,
          stream,
          parser.Files[ 0 ].FileName,
          cancel );
      }

      else if ( files.Contains( "map.xml" ) )
      {
        Logger.LogInformation( "Detected OLab3 import file" );

        mapPhys = await _endpoint3.ImportAsync(
          auth,
          stream,
          parser.Files[ 0 ].FileName,
          cancel );
      }

      else
        throw new Exception( "Invalid archive file" );

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
}
