using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.Api.Model;
using System;
using System.Net;
using System.Threading.Tasks;
using OLab.Azure.Utils;
using OLab.Azure.Services;
using OLab.Azure.Extensions;
using OLab.Common.Utils;
using OLab.Api.Utils;

namespace OLab.Azure.Middleware;

public class OLabAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;
  private readonly OLabDBContext _dbContext;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    IOLabAuthentication authentication,
    OLabDBContext dbContext)
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
    Guard.Argument( authentication ).NotNull( nameof( authentication ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "OLabAuthMiddleware created" );

    _config = configuration;
    _dbContext = dbContext;
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

      var contextInfo = executionContext.Items[ "contextHelper" ] as ContextHelper;
      Guard.Argument( contextInfo ).NotNull( nameof( contextInfo ) );

      try
      {
        var authentication = new OLabAuthentication( _logger, _config, _dbContext );
        var token = authentication.ExtractAccessToken( contextInfo.Headers, contextInfo.BindingData );

        authentication.ValidateToken( token );

        // these must be set before building UserContextService 
        executionContext.Items.Add( "headers", contextInfo.Headers );
        executionContext.Items.Add( "claims", authentication.Claims );

        // This is added pre-function execution, function will have access to this information
        var userContext = new FunctionUserContextService( _logger, executionContext, _dbContext );
        executionContext.Items.Add( "usercontext", userContext );

        // This happens after function execution. We can inspect the context after the function
        // was invoked
        //if (executionContext.Items.TryGetValue("functionitem", out var value) && value is string message)
        //  _logger.LogInformation($"From function: {message}");

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