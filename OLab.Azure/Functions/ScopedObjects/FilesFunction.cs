using Azure;
using Dawn;
using FluentValidation;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.ScopedObjects;

public class FilesFunction : OLabFunction
{
  private readonly FilesEndpoint _endpoint;

  /// <summary>
  /// Initializes a new instance of the <see cref="FilesFunction"/> class.
  /// </summary>
  /// <param name="loggerFactory">The logger factory.</param>
  /// <param name="configuration">The configuration.</param>
  /// <param name="dbContext">The database context.</param>
  /// <param name="wikiTagProvider">The wiki tag provider.</param>
  /// <param name="fileStorageProvider">The file storage provider.</param>
  public FilesFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base( configuration, dbContext )
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    Logger = OLabLogger.CreateNew<FilesFunction>( loggerFactory );
    _endpoint = new FilesEndpoint( Logger, configuration, dbContext, wikiTagProvider, fileStorageProvider );
  }

  /// <summary>
  /// Capitalizes the first letter of the given string.
  /// </summary>
  /// <param name="str">The string to capitalize.</param>
  /// <returns>The string with the first letter capitalized.</returns>
  private static string CapitalizeFirstLetter(string str)
  {
    if ( str.Length == 0 )
      return str;

    if ( str.Length == 1 )
      return char.ToUpper( str[ 0 ] ).ToString();
    else
      return char.ToUpper( str[ 0 ] ) + str.Substring( 1 );
  }

  /// <summary>
  /// Builds the static file name.
  /// </summary>
  /// <param name="dto">The file DTO.</param>
  /// <returns>The static file name.</returns>
  private string BuildStaticFileName(FilesFullDto dto)
  {
    string tempFileName;

    var dirName = Path.Combine(
      CapitalizeFirstLetter( dto.ImageableType ),
      dto.ImageableId.ToString() );

    tempFileName = Path.Combine( dirName, dto.Name );

    Logger.LogInformation( $"Static file name: {tempFileName}" );

    return tempFileName;
  }

  /// <summary>
  /// Gets the form field helper asynchronously.
  /// </summary>
  /// <param name="stream">The stream.</param>
  /// <param name="parser">The multipart form data parser.</param>
  /// <returns>The form field helper.</returns>
  private IOLabFormFieldHelper GetFormFieldHelperAsync(Stream stream, MultipartFormDataParser parser)
  {
    var helper = new OLabFormFieldHelper( stream );

    helper.Fields.Add( "id", Convert.ToUInt32( parser.GetParameterValue( "id" ) ) );
    helper.Fields.Add( "name", parser.GetParameterValue( "name" ) );
    helper.Fields.Add( "description", parser.GetParameterValue( "description" ) );
    helper.Fields.Add( "copyright", parser.GetParameterValue( "copyright" ) );
    helper.Fields.Add( "parentId", Convert.ToUInt32( parser.GetParameterValue( "parentId" ) ) );
    helper.Fields.Add( "scopeLevel", parser.GetParameterValue( "scopeLevel" ) );
    helper.Fields.Add( "isMediaResource", Convert.ToBoolean( parser.GetParameterValue( "isMediaResource" ) ) );
    helper.Fields.Add( "selectedFileName", parser.GetParameterValue( "selectedFileName" ) );
    helper.Fields.Add( "fileSize", Convert.ToInt32( parser.GetParameterValue( "fileSize" ) ) );

    Logger.LogInformation( $"Form fields:" );

    foreach ( var field in helper.Fields )
      Logger.LogInformation( $"  {field.Key} = {field.Value}" );

    helper.Stream = parser.Files[ 0 ].Data;
    helper.Stream.Position = 0;

    if ( helper.Stream.Length > 0 )
      Logger.LogInformation( $"  file: {helper.Field( "selectedFileName" )}. size {helper.Stream.Length}" );

    return helper;
  }

  /// <summary>
  /// Gets all files.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="executionContext">The function context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The action result.</returns>
  [Function( "FilesGet" )]
  public async Task<IActionResult> FilesGetAsync(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "files" )] HttpRequestData request,
      FunctionContext executionContext,
      CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"FilesGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<FilesDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( FilesGetAsync ) );
    }

  }

  /// <summary>
  /// Gets a single file by ID.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="id">The file ID.</param>
  /// <returns>The action result.</returns>
  [Function( "FileGet" )]
  public async Task<IActionResult> FileGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "files/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id
  )
  {
    Guard.Argument( request ).NotNull( nameof( request ) );

    try
    {
      Logger.LogInformation( $"FileGet" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var dto = await _endpoint.GetAsync( auth, id );
      var blobName = BuildStaticFileName( dto );

      return request
        .CreateResponse( OLabObjectResult<FilesFullDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( FileGetAsync ) );
    }
  }

  /// <summary>
  /// Creates a new file.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function context.</param>
  /// <param name="token">The cancellation token.</param>
  /// <returns>The action result.</returns>
  [Function( "FilePost" )]
  public async Task<IActionResult> FilePostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "files" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken token)
  {
    var fileName = "";

    try
    {
      Logger.LogInformation( $"FilePost" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );

      var parser = await MultipartFormDataParser.ParseAsync( request.Body ).ConfigureAwait( false );

      using ( var stream = new MemoryStream() )
      {
        var formHelper = GetFormFieldHelperAsync( stream, parser );

        var dto = new FilesFullDto( formHelper );
        dto = await _endpoint.PostAsync( auth, dto, token );

        return request
          .CreateResponse( OLabObjectResult<FilesFullDto>.Result( dto ) );
      }

    }
    catch ( Exception ex )
    {
      if ( ex is RequestFailedException )
      {
        var azureException = ex as RequestFailedException;
        if ( azureException.Status == 409 )
          return request
            .CreateResponse( OLabServerErrorResult.Result(
              $"File '{fileName}' already exists",
              HttpStatusCode.Conflict ) );
        else
          return request
            .CreateResponse( OLabServerErrorResult.Result(
              $"Error creating static file '{fileName}'.  {ex.Message}",
              (HttpStatusCode)azureException.Status ) );
      }
      else
        return request
          .CreateResponse( OLabServerErrorResult.Result(
              $"Error creating static file '{fileName}'.  {ex.Message}",
              HttpStatusCode.InternalServerError ) );
    }
  }

  /// <summary>
  /// Deletes a file by ID.
  /// </summary>
  /// <param name="request">The HTTP request data.</param>
  /// <param name="hostContext">The function context.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="id">The file ID.</param>
  /// <returns>The action result.</returns>
  [Function( "FileDelete" )]
  public async Task<IActionResult> DeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "files/{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Logger.LogInformation( $"FileDelete" );

      // validate token/setup up common properties
      var auth = GetAuthorization( hostContext );
      await _endpoint.DeleteAsync( auth, id );

      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( DeleteAsync ) );
    }

  }

  /// <summary>
  /// Saves a object edit
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns>IActionResult</returns>
  [Function( "FilePut" )]
  public async Task<IActionResult> FilePutAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "fi/les{id}" )] HttpRequestData request,
    FunctionContext hostContext, CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Guard.Argument( request ).NotNull( nameof( request ) );
      Guard.Argument( hostContext ).NotNull( nameof( hostContext ) );
      Guard.Argument( id, nameof( id ) ).NotZero();

      var auth = GetAuthorization( hostContext );

      var body = await request.ParseBodyFromRequestAsync<FilesFullDto>();

      await _endpoint.PutAsync( auth, id, body );
      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( FilePutAsync ) );
    }

  }
}
