using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[Route("olab/api/v3/acls")]
[ApiController]
public class GroupRoleAclsController : OLabController
{
  private readonly GroupRoleAclsEndpoint _endpoint;

  public GroupRoleAclsController(ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  OLabDBContext dbContext,
  IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
  IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
    configuration,
    dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<GroupRoleAclsController>(loggerFactory);

    _endpoint = new GroupRoleAclsEndpoint(
      Logger,
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Retrieve all group objects
  /// </summary>
  /// <param name="model">group, role, map and node ids to search</param>
  /// <returns>Array of file records</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync(GroupRoleAclRequest model)
  {
    try
    {
      Logger.LogDebug($"GroupRoleAclsEndpoint.GetAsync");

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      if ( !await auth.IsSystemSuperuserAsync() )
        throw new OLabUnauthorizedException();

      var items = await _endpoint.GetAsync(auth, model);
      return HttpContext.Request
        .CreateResponse(OLabObjectListResult<GroupRoleAclDto>.Result(items));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }
}
