using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Azure.Extensions;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Contracts;
using OLab.Data.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions.Management;

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
  public async Task<HttpResponseData> GroupRolesAclQueryAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "acls" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclQueryAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var body = await request.ParseBodyFromRequestAsync<GroupRoleAclReadRequest>( GetLogger() );

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
  public async Task<HttpResponseData> GroupRolesAclEditAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "put", Route = "acl" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclEditPut" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var model = await request.ParseBodyFromRequestAsync<GroupRoleAclDto>( GetLogger() );

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
  public async Task<HttpResponseData> GroupRolesAclDeleteAsync(
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

      return OLabFunctionResponses.OLabNoContentResponse( request );
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
  public async Task<HttpResponseData> GroupRolesAclCreateAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "acl" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclCreateAsync" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var model = await request.ParseBodyFromRequestAsync<GroupRoleAclDto>( GetLogger() );

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
