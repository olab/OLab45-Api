using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using IOLabAuthentication = OLab.Api.Data.Interface.IOLabAuthentication;

namespace OLab.FunctionApp.Functions;

public class OLabFunction
{
  protected readonly OLabDBContext DbContext;
  protected HttpResponseData response;

  ///  this is set in derived classes
  protected IOLabLogger Logger = null;

  protected string Token;
  protected readonly IUserService userService;
  protected IUserContext userContext;
  protected readonly IOLabConfiguration _configuration;
  protected readonly IOLabModuleProvider<IWikiTagModule> _wikiTagModules;

  public OLabFunction(
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _configuration = configuration;

    DbContext = dbContext;
    this.userService = userService;
  }

  public OLabFunction(
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagModules)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(wikiTagModules).NotNull(nameof(wikiTagModules));

    _configuration = configuration;
    _wikiTagModules = wikiTagModules;

    DbContext = dbContext;
    this.userService = userService;
  }

  /// <summary>
  /// Get the authentication context from the host context
  /// </summary>
  /// <param name="hostContext">Function context</param>
  /// <returns>IOLabAuthentication</returns>
  /// <exception cref="Exception"></exception>
  protected IOLabAuthentication GetRequestContext(FunctionContext hostContext)
  {
    // Get the item set by the middleware
    if (hostContext.Items.TryGetValue("auth", out var value) && value is IOLabAuthentication auth)
      Logger.LogInformation("Got auth context");
    else
      throw new Exception("unable to get authentication context");

    return auth;
  }
}