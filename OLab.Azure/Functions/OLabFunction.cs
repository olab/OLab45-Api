using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Azure.Services;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public class OLabFunction
{
  protected readonly OLabDBContext DbContext;
  protected HttpResponseData response;

  //  this is set in derived classes
  protected IOLabLogger Logger = null;

  protected string Token;
  //protected readonly IUserService userService;
  protected IUserContext userContext;
  protected readonly IOLabConfiguration _configuration;
  //protected readonly TTalkDBContext TtalkDbContext;
  protected readonly IOLabModuleProvider<IWikiTagModule> _wikiTagProvider;
  protected readonly IOLabModuleProvider<IFileStorageModule> _fileStorageProvider;

  protected ILogger GetLogger() { return Logger.GetLogger(); }

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
    if ( executionContext.Items.TryGetValue( nameof( FunctionAppUserContext ), out var value ) && (value is IUserContext userContext) )
    {
      GetLogger().LogInformation( $"User context: {userContext}" );

      var auth = new OLabAuthorization( Logger, DbContext, _configuration );
      auth.ApplyUserContextAsync( userContext ).GetAwaiter().GetResult();

      return auth;
    }

    throw new Exception( "unable to get executionContext item usercontext" );

  }

  /// <summary>
  /// ReadAsync question with responses
  /// </summary>
  /// <param name="id">question id</param>
  /// <returns></returns>
  [NonAction]
  protected async ValueTask<SystemQuestions> GetQuestionAsync(uint id)
  {
    var item = await DbContext.SystemQuestions
        .Include( x => x.SystemQuestionResponses )
        .FirstOrDefaultAsync( x => x.Id == id );
    return item;
  }

}