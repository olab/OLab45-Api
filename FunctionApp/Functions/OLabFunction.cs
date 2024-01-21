using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Models;
using OLab.Common.Interfaces;
using OLab.Data.Exceptions;
using OLab.Data.Interface;
using OLab.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
  //protected readonly TTalkDBContext TtalkDbContext;
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

  public async Task<uint> GetRootNodeId(uint mapId)
  {
    var phys = await DbContext
      .MapNodes
      .FirstOrDefaultAsync(x => x.MapId == mapId && x.TypeId.Value == (int)MapNodes.NodeType.RootNode);

    if (phys == null)
    {
      // if no map node by this point, then the map doesn't have a root node
      // defined so take the first one (by id)        
      phys = await DbContext.MapNodes
        .Where(x => x.MapId == mapId)
        .OrderBy(x => x.Id)
        .FirstOrDefaultAsync();
    }

    if (phys == null)
      throw new OLabObjectNotFoundException("MapNodes", mapId);

    return phys.Id;
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