using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public partial class GroupRoleAcls : OLabFunction
{
  private readonly GroupRoleAclsEndpoint _endpoint;

  public GroupRoleAcls(
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

    _endpoint = new GroupRoleAclsEndpoint(
      Logger,
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider );
  }

  /// <summary>
  /// Get single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "GroupRolesAclQuery" )]
  public async Task<IActionResult> GroupRolesAclQueryAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "acls" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclQueryPost" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var body = await request.ParseBodyFromRequestAsync<GroupRoleAclReadRequest>();

      var dto = await _endpoint.GetAsync( auth, body );
      return request
        .CreateResponse( OLabObjectListResult<GroupRoleAclDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupRolesAclQueryAsync ) );
    }

  }

  /// <summary>
  /// Get single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "GroupRolesAclEdit" )]
  public async Task<IActionResult> GroupRolesAclEditAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "acl" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclEditPut" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var model = await request.ParseBodyFromRequestAsync<GroupRoleAclDto>();
      var dto = await _endpoint.EditAsync( auth, model );

      return request
        .CreateResponse( OLabObjectResult<GroupRoleAclDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupRolesAclEditAsync ) );
    }

  }

  /// <summary>
  /// Get single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "GroupRolesAclDelete" )]
  public async Task<IActionResult> GroupRolesAclDeleteAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "acl/{id}" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken,
    uint id)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclDelete" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      await _endpoint.DeleteAsync( auth, id );

      return new NoContentResult();
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupRolesAclDeleteAsync ) );
    }

  }

  /// <summary>
  /// Create single object
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [Function( "GroupRolesAclCreate" )]
  public async Task<IActionResult> GroupRolesAclCreateAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "acl" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclCreatePost" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var model = await request.ParseBodyFromRequestAsync<GroupRoleAclDto>();
      var dto = await _endpoint.CreateAsync( auth, model );

      return request
        .CreateResponse( OLabObjectResult<GroupRoleAclDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      return ProcessException( request, ex, nameof( GroupRolesAclCreateAsync ) );
    }

  }

}
