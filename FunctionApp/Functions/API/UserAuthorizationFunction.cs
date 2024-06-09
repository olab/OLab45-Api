using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;
using OLab.Data.Interface;

namespace OLab.FunctionApp.Functions.API;

public class UserAuthorizationFunction : OLabFunction
{
  private readonly UserAuthorizationEndpoint _endpoint;

  public UserAuthorizationFunction(
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

    Logger = OLabLogger.CreateNew<UserAuthorizationFunction>(loggerFactory);

    _endpoint = new UserAuthorizationEndpoint(
      Logger,
      configuration,
      DbContext,
      wikiTagProvider,
      fileStorageProvider);
  }

  /// <summary>
  /// Get user's groups
  /// </summary>
  /// <param name="token"></param>
  /// <returns>List of user's current groups</returns>
  [Function("GetUserGroups")]
  public async Task<HttpResponseData> GetUserGroupsAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/user/groups")] HttpRequestData request,
      FunctionContext hostContext,
      CancellationToken token)
  {
    Guard.Argument(request).NotNull(nameof(request));
    Guard.Argument(hostContext).NotNull(nameof(hostContext));

    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var userResponse = await _endpoint.GetUserGroups(auth, token);
      response = 
        request.CreateResponse(OLabObjectListResult<GroupsDto>.Result(userResponse));

    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }

  /// <summary>
  /// Adds a group/role to a user
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>List of user's current group/roles</returns>
  [Function("AddUserGroupsAsync")]
  public async Task<HttpResponseData> AddUserGroupsAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/user/groups")] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(hostContext).NotNull(nameof(hostContext));

      var body = await request.ParseBodyFromRequestAsync<UserGroupRolesDto>();

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var userResponse = await _endpoint.AddAsync(auth, body, token);
      response = 
        request.CreateResponse(OLabObjectListResult<UserGroupRolesDto>.Result(userResponse));
    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }

  /// <summary>
  /// Removes a group/role for a user
  /// </summary>
  /// <param name="request">ImportRequest</param>
  /// <returns>List of user's current group/roles</returns>
  [Function("DeleteUserGroupAsync")]
  public async Task<HttpResponseData> DeleteUserGroupAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "auth/user/groups")] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken token)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(hostContext).NotNull(nameof(hostContext));

      // validate token/setup up common properties
      var auth = GetAuthorization(hostContext);

      var dto = await request.ParseBodyFromRequestAsync<UserGroupRolesDto>();

      var userResponse = await _endpoint.DeleteAsync(auth, dto, token);
      response = 
        request.CreateResponse(OLabObjectListResult<UserGroupRolesDto>.Result(userResponse));
    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }
}
