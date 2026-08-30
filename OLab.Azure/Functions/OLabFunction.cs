using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Data.Exceptions;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Services;
using OLab.Azure.Utils;
using OLab.Common.Exceptions;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public class OLabFunction
{
  protected readonly OLabDBContext DbContext;
  protected HttpResponseData response;

  //  this is set in derived classes
  protected IOLabLogger Logger = null;

  protected string Token;
  protected IAuthenticatedContext userContext;
  protected readonly IOLabConfiguration _configuration;
  protected readonly IOLabModuleProvider<IWikiTagModule> _wikiTagProvider;
  protected readonly IOLabModuleProvider<IFileStorageModule> _fileStorageProvider;

  protected IOLabLogger GetLogger() { return Logger; }

  public OLabFunction(
    IOLabConfiguration configuration,
    OLabDBContext dbContext)
  {
    Guard.Argument( configuration ).NotNull( nameof( configuration ) );
    Guard.Argument( dbContext ).NotNull( nameof( dbContext ) );

    _configuration = configuration;

    DbContext = dbContext;
  }

  public OLabFunction(
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : this( configuration, dbContext )
  {
    Guard.Argument( wikiTagProvider ).NotNull( nameof( wikiTagProvider ) );
    Guard.Argument( fileStorageProvider ).NotNull( nameof( fileStorageProvider ) );

    _wikiTagProvider = wikiTagProvider;
    _fileStorageProvider = fileStorageProvider;

  }

  /// <summary>
  /// Builds the authentication context from the host context
  /// </summary>
  /// <param name="executionContext">Function context</param>
  /// <returns>IOLabAuthentication</returns>
  /// <exception cref="Exception"></exception>
  protected IOLabAuthorization GetAuthorization(FunctionContext executionContext)
  {
    var items = executionContext.Items.Select( x => x.Key );
    GetLogger().LogInformation( $"GetAuthorization executionContext items {string.Join( ", ", items )}" );

    // Get the user context set by the middleware
    if ( executionContext.Items.TryGetValue( nameof( AuthenticatedMiddlewareContext ), out var value ) && (value is IAuthenticatedContext authenticatedContext) )
    {
      GetLogger().LogInformation( $"User context: {authenticatedContext}" );

      var auth = new OLabAuthorization( Logger, DbContext, _configuration );
      auth.ApplyUserContextAsync( authenticatedContext ).GetAwaiter().GetResult();

      return auth;
    }

    throw new OLabUnauthorizedException( "unable to authorize request" );

  }

  /// <summary>
  /// ReadAsync question with responses
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns></returns>
  protected async ValueTask<SystemQuestions> GetQuestionAsync(uint id)
  {
    var item = await DbContext.SystemQuestions
        .Include( x => x.SystemQuestionResponses )
        .FirstOrDefaultAsync( x => x.Id == id );
    return item;
  }

  protected static (int? take, int? skip) ExtractPageParameters(HttpRequestData request)
  {
    int? take;
    int? skip;

    var queryTake = Convert.ToInt32( request.Query[ "take" ] );
    var querySkip = Convert.ToInt32( request.Query[ "skip" ] );
    take = queryTake > 0 ? queryTake : null;
    skip = querySkip > 0 ? querySkip : null;

    return (take, skip);
  }

  protected HttpResponseData ProcessException(HttpRequestData request, Exception ex, string caller)
  {
    Logger.LogError( $"{caller} exception: {ex.Message}" );
    Logger.LogError( $"{caller} {ex.StackTrace}" );

    // 404 Not Found
    if ( ex is OLabObjectNotFoundException )
      return OLabFunctionResponses.OLabNotFoundResponse( request );

    // 401 Unauthorized
    if ( ex is OLabUnauthorizedException )
      return OLabFunctionResponses.OLabUnauthorizedResponse( request );

    // 500 Internal Server Error (your existing error wrapper)
    var serverError = OLabServerErrorResult.Result( ex );

    var errorResponse = request.CreateResponse( HttpStatusCode.InternalServerError );
    errorResponse.Headers.Add( "Content-Type", "application/json" );
    errorResponse.WriteString( JsonConvert.SerializeObject( serverError ) );

    return errorResponse;
  }

}