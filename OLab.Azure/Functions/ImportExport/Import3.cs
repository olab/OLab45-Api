using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Endpoints;

namespace OLab.Azure.Functions.ImportExport;

public class Import3Function : OLabFunction
{
  private readonly Import3Endpoint _endpoint;

  public Import3Function(
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

    Logger = OLabLogger.CreateNew<Import3Function>( loggerFactory, true );

    _endpoint = new Import3Endpoint(
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
  [Function( "Import3" )]
  public async Task<IActionResult> ImportAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "import3" )] HttpRequestData request,
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

      var dto = new ImportResponse
      {
        Id = mapPhys.Id,
        Name = mapPhys.Name,
        CreatedAt = mapPhys.CreatedAt.Value,
        LogMessages = Logger.GetMessages( OLabLogMessage.MessageLevel.Info ).Select( x => x.Message ).ToList()
      };

      var result = OLabObjectResult<ImportResponse>.Result( dto );
      result.Message = Logger.HasErrorMessage() ? "error" : "success";
      return request
        .CreateResponse( result );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "MapGetShortStatusAsync" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}

