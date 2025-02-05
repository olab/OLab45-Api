using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Azure.Extensions;
using OLab.Azure.Services;
using OLab.Azure.Utils;
using OLab.Common.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OLab.Azure.Middleware;

/// <summary>
/// Middleware for handling authenticated requests.
/// </summary>
public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IOLabAuthentication authentication)
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
    Guard.Argument( authentication ).NotNull( nameof( authentication ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "OLabAuthMiddleware created" );

    _config = configuration;
  }

  public static bool CanInvoke(FunctionContext executionContext)
  {
    var httpRequestData = executionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
    var haveAuthorization = httpRequestData.Headers.Contains( "authorization" );
    return haveAuthorization;
  }

  public async Task Invoke(
    FunctionContext executionContext,
    FunctionExecutionDelegate next)
  {
    Guard.Argument( executionContext ).NotNull( nameof( executionContext ) );

    try
    {
      _logger.LogInformation( "OLabAuthMiddleware invoke" );

      var executionContextHelper = executionContext.Items[ nameof( ExecutionContextHelper ) ] as ExecutionContextHelper;
      Guard.Argument( executionContextHelper ).NotNull( nameof( executionContextHelper ) );

      try
      {

        var dbContext = executionContext.InstanceServices.GetRequiredService<OLabDBContext>();

        var authentication = new OLabAuthentication( _logger, _config, dbContext );
        var token
          = OLabAuthentication.ExtractAccessToken( executionContextHelper.Request, false );
        authentication.ValidateToken( token );

        // these must be set before building UserContextService 
        executionContext.Items.Add( "claims", authentication.Claims );

        // This is added pre-function execution, function will have access to this information
        var userContext = new FunctionAppUserContext( _logger, executionContext, dbContext );
        executionContext.Items.Add( nameof( FunctionAppUserContext ), userContext );

        // This happens after function execution. We can inspect the context after the function
        // was invoked
        //if (executionContext.Items.TryGetValue("functionitem", out var value) && value is string message)
        //  _logger.LogInformation($"From function: {message}");

        var items = executionContext.Items.Select( x => x.Key );
        _logger.LogInformation( $"OLabAuthMiddleware executionContext items: {string.Join( ", ", items )}" );

      }
      catch ( Exception ex )
      {
        _logger.LogError( $"function error: {ex.Message} {ex.StackTrace}" );
        await executionContext.CreateJsonResponse( HttpStatusCode.Unauthorized, new { Message = "could not process token." } );
      }

      await next( executionContext );

    }
    catch ( Exception ex )
    {
      _logger.LogError( $"OLabAuthMiddleware error: {ex.Message} {ex.StackTrace}" );
      await executionContext.CreateJsonResponse( HttpStatusCode.InternalServerError, ex.Message );
    }

  }

}