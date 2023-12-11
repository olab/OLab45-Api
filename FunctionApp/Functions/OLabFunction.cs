using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data.Interface;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Access;
using System.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using OLab.Data.BusinessObjects;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.FunctionApp.Functions;

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
  protected readonly TTalkDBContext TtalkDbContext;
  protected readonly IOLabModuleProvider<IWikiTagModule> _wikiTagProvider;
  protected readonly IOLabModuleProvider<IFileStorageModule> _fileStorageProvider;

  public OLabFunction(
    IOLabConfiguration configuration,
    OLabDBContext dbContext)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _configuration = configuration;

    DbContext = dbContext;
  }

  public OLabFunction(
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : this(configuration, dbContext)
  {
    Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));
    Guard.Argument(fileStorageProvider).NotNull(nameof(fileStorageProvider));

    _wikiTagProvider = wikiTagProvider;
    _fileStorageProvider = fileStorageProvider;

  }

  public OLabFunction(
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : this(configuration, dbContext)
  {
    Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));
    Guard.Argument(fileStorageProvider).NotNull(nameof(fileStorageProvider));
    TtalkDbContext = ttalkDbContext;
    _wikiTagProvider = wikiTagProvider;
    _fileStorageProvider = fileStorageProvider;

  }

  /// <summary>
  /// Centralized processing of exceptions
  /// </summary>
  /// <param name="ex"></param>
  /// <exception cref="NotImplementedException"></exception>
  protected void ProcessException(Exception ex)
  {
    Logger.LogError($"{ex.Message}");

    var inner = ex.InnerException;
    while (inner != null)
    {
      Logger.LogError($"  {inner.Message}");
      inner = inner.InnerException;
    }

    Logger.LogError($"{ex.StackTrace}");

  }

  /// <summary>
  /// Builds the authentication context from the host context
  /// </summary>
  /// <param name="hostContext">Function context</param>
  /// <returns>IOLabAuthentication</returns>
  /// <exception cref="Exception"></exception>
  protected IOLabAuthorization GetAuthorization(FunctionContext hostContext)
  {
    // Get the user context set by the middleware
    if (hostContext.Items.TryGetValue("usercontext", out var value) && value is IUserContext userContext)
    {
      Logger.LogInformation($"User context: {userContext}");

      var auth = new OLabAuthorization(Logger, DbContext);
      auth.ApplyUserContext(userContext);

      return auth;
    }

    throw new Exception("unable to get auth RequestContext");

  }
}