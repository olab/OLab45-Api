using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Interface;

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

    Logger = OLabLogger.CreateNew<Servers>( loggerFactory );

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
  [Function( "GroupRolesAclPost" )]
  public async Task<IActionResult> GroupRolesAclPostAsync(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "acls" )] HttpRequestData request,
    FunctionContext executionContext,
    CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation( $"GroupRolesAclPost" );

      // validate token/setup up common properties
      var auth = GetAuthorization( executionContext );
      var body = await request.ParseBodyFromRequestAsync<GroupRoleAclRequest>();

      // test if user has access to add users.
      if ( !await auth.IsSystemSuperuserAsync() )
        return request.CreateResponse( OLabUnauthorizedObjectResult.Result( "Not authorized to post acls" ) );


      var dto = await _endpoint.GetAsync( auth, body );
      return request
        .CreateResponse( OLabObjectListResult<GroupRoleAclDto>.Result( dto ) );
    }
    catch ( Exception ex )
    {
      Logger.LogError( ex, "GroupRolesAclPost" );

      return request
        .CreateResponse( OLabServerErrorResult.Result( ex ) );
    }

  }

}
