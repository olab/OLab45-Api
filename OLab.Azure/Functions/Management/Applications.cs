using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Management;

public partial class ApplicationsFunction : OLabFunction
{
  private readonly ApplicationsEndpoint _endpoint;

  public ApplicationsFunction(
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

    Logger = OLabLogger.CreateNew<ServersFunction>( loggerFactory );

    _endpoint = new ApplicationsEndpoint(
      Logger,
      configuration,
      DbContext );
  }

  /// <summary>
  /// ReadAsync a list of Roles
  /// </summary>
  /// <param name="take">Max number of records to return</param>
  /// <param name="skip">SKip over a number of records</param>
  /// <returns>IActionResult</returns>
  [Function( "ApplicationsGet" )]
  public async Task<HttpResponseData> ApplicationsGetAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "applications" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"ApplicationsGet" );

      var pageSpecs = ExtractPageParameters( request );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var result = await _endpoint.GetAsync( auth, pageSpecs.take, pageSpecs.skip );

      return request
        .CreateResponse( OLabObjectPagedListResult<ApplicationsDto>.Result( result.Data, result.Remaining ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( ApplicationsGetAsync ) );
    }

  }

}
