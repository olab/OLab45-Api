using Data.Contracts;
using Dawn;
using Endpoints.player.ReportEndpoint;
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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public partial class ReportFunction : OLabFunction
{
  private readonly ReportEndpoint _endpoint;

  public ReportFunction(
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
    Guard.Argument( wikiTagProvider ).NotNull( nameof( wikiTagProvider ) );
    Guard.Argument( fileStorageProvider ).NotNull( nameof( fileStorageProvider ) );

    Logger = OLabLogger.CreateNew<ReportFunction>( loggerFactory );

    _endpoint = new ReportEndpoint(
      Logger,
      configuration,
      DbContext );
  }

  /// <summary>
  /// Read a list of object
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "GetReport" )]
  public async Task<IActionResult> GetReportAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "reports/{sessionId}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    string sessionId)
  {
    try
    {
      Logger.LogInformation( $"GetReport" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var response = await _endpoint.GetAsync( auth, sessionId );

      return request
        .CreateResponse( OLabObjectResult<SessionReport>.Result( response ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GetReportAsync ) );
    }

  }

}
