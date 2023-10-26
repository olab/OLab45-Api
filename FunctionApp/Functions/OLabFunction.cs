using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.Data.Interface;

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
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : this( configuration, dbContext )
  {
    Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));
    Guard.Argument(fileStorageProvider).NotNull(nameof(fileStorageProvider));

    _wikiTagProvider = wikiTagProvider;
    _fileStorageProvider = fileStorageProvider;

  }

  /// <summary>
  /// Get the _authentication context from the host context
  /// </summary>
  /// <param name="hostContext">Function context</param>
  /// <returns>IOLabAuthentication</returns>
  /// <exception cref="Exception"></exception>
  protected IOLabAuthorization GetRequestContext(FunctionContext hostContext)
  {
    // Get the item set by the middleware
    if (hostContext.Items.TryGetValue("auth", out var value) && value is IOLabAuthorization auth)
      Logger.LogInformation("Got auth RequestContext");
    else
      throw new Exception("unable to get auth RequestContext");

    return auth;
  }
}