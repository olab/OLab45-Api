using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi;

[Route("olab/api/v3/auth/user")]
[ApiController]
public class UserAuthorizationController : OLabController
{
  private readonly UserAuthorizationEndpoint _endpoint;

  public UserAuthorizationController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<ImportController>(loggerFactory, true);

    _endpoint = new UserAuthorizationEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Adds a group/role to a user
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>List of user's current group/roles</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> AddAsync(
    UserGroupRolesDto dto,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(dto).NotNull(nameof(dto));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var response = await _endpoint.AddAsync(auth, dto, token);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<UserGroupRolesDto>.Result(response));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Removes a group/role for a user
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>List of user's current group/roles</returns>
  [HttpDelete]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> DeleteAsync(
    UserGroupRolesDto dto,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(dto).NotNull(nameof(dto));

      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var response = await _endpoint.DeleteAsync(auth, dto, token);
      return HttpContext.Request.CreateResponse(OLabObjectListResult<UserGroupRolesDto>.Result(response));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }
}
