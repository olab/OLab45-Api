using Dawn;
using OLab.Access.Interfaces;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Dtos.ScopedObjects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OLab.Access;

/// <summary>
/// Represents a session in the OLab application.
/// </summary>
public class OLabSession : IOLabSession
{
  private readonly OLabDBContext _dbContext;
  private readonly IAuthenticatedContext _userContext;
  private readonly IOLabLogger _logger;
  private string _sessionId;
  private uint _mapId;

  /// <summary>
  /// Creates an instance of <see cref="OLabSession"/>.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="context">The database context.</param>
  /// <param name="userContext">The user context.</param>
  /// <returns>An instance of <see cref="IOLabSession"/>.</returns>
  public static IOLabSession CreateInstance(
    IOLabLogger logger,
    OLabDBContext context,
    IAuthenticatedContext userContext)
  {
    return new OLabSession( logger, context, userContext );
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="OLabSession"/> class.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="context">The database context.</param>
  /// <param name="userContext">The user context.</param>
  public OLabSession(
    IOLabLogger logger,
    OLabDBContext context,
    IAuthenticatedContext userContext)
  {
    Guard.Argument( logger, nameof( logger ) ).NotNull();
    Guard.Argument( context, nameof( context ) ).NotNull();
    Guard.Argument( userContext, nameof( userContext ) ).NotNull();

    _dbContext = context;
    _userContext = userContext;
    _logger = logger;

    if ( !string.IsNullOrEmpty( _userContext.SessionId ) )
      SetSessionId( _userContext.SessionId );
  }

  /// <summary>
  /// Sets the map ID for the session.
  /// </summary>
  /// <param name="mapId">The map ID.</param>
  public void SetMapId(uint mapId)
  {
    Guard.Argument( mapId, nameof( mapId ) ).Positive();
    _mapId = mapId;
  }

  /// <summary>
  /// Sets the session ID.
  /// </summary>
  /// <param name="sessionId">The session ID.</param>
  public void SetSessionId(string sessionId)
  {
    Guard.Argument( sessionId, nameof( sessionId ) ).NotNull();
    _sessionId = sessionId;
  }

  /// <summary>
  /// Gets the session ID.
  /// </summary>
  /// <returns>The session ID.</returns>
  public string GetSessionId()
  {
    return _sessionId;
  }

  /// <summary>
  /// Creates a new session on the map.
  /// </summary>
  public void OnStartSession()
  {
    Guard.Argument( _mapId, nameof( _mapId ) ).Positive();

    SetSessionId( IOLabSession.GenerateSessionId() );
    _logger.LogInformation( $"generated a new session Id: {GetSessionId()}" );

    var session = new UserSessions
    {
      Uuid = GetSessionId(),
      MapId = _mapId,
      StartTime = Conversions.GetCurrentUnixTime(),
      UserIp = _userContext.IPAddress,
      Iss = _userContext.Issuer,
      UserId = _userContext.UserId,
      CourseName = _userContext.ReferringCourse
    };

    _dbContext.UserSessions.Add( session );
    _dbContext.SaveChanges();

    _logger.LogInformation( $"OnStartSession: session {GetSessionId()} ({_userContext.UserName}) MapId: {_mapId}. Session PK: {session.Id}" );
  }

  /// <summary>
  /// Extends the session end time.
  /// </summary>
  /// <param name="nodeId">The node ID.</param>
  public void OnExtendSessionEnd(uint nodeId)
  {
    Guard.Argument( _mapId, nameof( _mapId ) ).Positive();

    _logger.LogInformation( $"OnExtendSession: session {GetSessionId()} Map: {_mapId} Node: {nodeId}" );

    var session = GetSessionFromDatabase( GetSessionId() );
    if ( session == null )
      return;

    session.EndTime = Conversions.GetCurrentUnixTime();

    _dbContext.UserSessions.Update( session );
    _dbContext.SaveChanges();
  }

  /// <summary>
  /// Records a node play on the session.
  /// </summary>
  /// <param name="dto">The data transfer object containing node information.</param>
  public void OnPlayNode(MapsNodesFullRelationsDto dto)
  {
    Guard.Argument( _mapId, nameof( _mapId ) ).Positive();

    var nodeId = dto.Id.Value;
    _logger.LogInformation( $"OnPlayNode: session {GetSessionId()} Map: {_mapId} Node: {nodeId}" );

    var session = GetSessionFromDatabase( GetSessionId() );
    if ( session == null )
      return;

    var countersDto = new List<CounterValueDto>();

    foreach ( var counterDto in dto.DynamicObjects.Counters )
      countersDto.Add( new CounterValueDto( counterDto ) );

    var counterJson = JsonSerializer.Serialize( countersDto );

    var sessionTrace = new UserSessiontraces
    {
      SessionId = session.Id,
      MapId = _mapId,
      NodeId = nodeId,
      DateStamp = Conversions.GetCurrentUnixTime(),
      UserId = session.UserId,
      Counters = counterJson
    };

    _dbContext.UserSessiontraces.Add( sessionTrace );
    _dbContext.SaveChanges();

    var counterUpdate = new UserCounterUpdate
    {
      CounterState = dto.DynamicObjects.ToJson(),
    };

    _dbContext.UserCounterUpdate.Add( counterUpdate );
    _dbContext.SaveChanges();

    var userSessionTraceCounterUpdate = new UsersessiontraceCounterupdate
    {
      CounterupdateId = counterUpdate.Id,
      SessiontraceId = sessionTrace.Id
    };

    _dbContext.UsersessiontraceCounterupdate.Add( userSessionTraceCounterUpdate );
    _dbContext.SaveChanges();
  }

  /// <summary>
  /// Records a question response on the session.
  /// </summary>
  /// <param name="body">The data transfer object containing the question response.</param>
  /// <param name="questionPhys">The physical question object.</param>
  public void OnQuestionResponse(
    QuestionResponsePostDataDto body,
    SystemQuestions questionPhys)
  {
    Guard.Argument( _mapId, nameof( _mapId ) ).Positive();
    Guard.Argument( body, nameof( body ) ).NotNull();
    Guard.Argument( questionPhys, nameof( questionPhys ) ).NotNull();

    var sessionPhys = GetSessionFromDatabase( GetSessionId() );
    if ( sessionPhys == null )
      return;

    _logger.LogInformation( $"OnQuestionResponse: session {GetSessionId()} Map: {_mapId} Node: {body.NodeId} Question: {questionPhys.Id} = {body.Value} " );

    if ( string.IsNullOrEmpty( body.Value ) && body.Value.Length > 1000 )
      body.Value = body.Value[ 997.. ] + "...";

    var userResponse = new UserResponses
    {
      SessionId = sessionPhys.Id,
      QuestionId = questionPhys.Id,
      Response = body.Value,
      NodeId = body.NodeId,
      CreatedAt = Conversions.GetCurrentUnixTime()
    };

    _dbContext.UserResponses.Add( userResponse );
    _dbContext.SaveChanges();

    var counterUpdate = new UserCounterUpdate
    {
      CounterState = body.DynamicObjects.ToJson(),
    };

    _dbContext.UserCounterUpdate.Add( counterUpdate );
    _dbContext.SaveChanges();

    var userResponseCounterUpdate = new UserresponseCounterupdate
    {
      CounterupdateId = counterUpdate.Id,
      UserresponseId = userResponse.Id
    };

    _dbContext.UserresponseCounterupdate.Add( userResponseCounterUpdate );
    _dbContext.SaveChanges();

    _logger.LogInformation( $"OnQuestionResponse: saved user response to session" );
  }

  /// <summary>
  /// Saves the session state.
  /// </summary>
  /// <param name="nodeId">The node ID.</param>
  /// <param name="dynamicObjects">The dynamic objects data transfer object.</param>
  public void SaveSessionState(uint nodeId, DynamicScopedObjectsDto dynamicObjects)
  {
    Guard.Argument( _mapId, nameof( _mapId ) ).Positive();

    var userState = new UserState
    {
      MapId = _mapId,
      MapNodeId = nodeId,
      UserId = _userContext.UserId,
      StateData = Encoding.ASCII.GetBytes( JsonSerializer.Serialize( dynamicObjects ) )
    };

    _dbContext.UserState.Add( userState );
    _dbContext.SaveChanges();
  }

  /// <summary>
  /// Retrieves the session database record.
  /// </summary>
  /// <param name="sessionId">The session ID.</param>
  /// <returns>The session database record.</returns>
  private UserSessions GetSessionFromDatabase(string sessionId)
  {
    Guard.Argument( sessionId ).NotNull( nameof( sessionId ) );

    var physSession = _dbContext.UserSessions.FirstOrDefault( x => x.Uuid == sessionId );
    if ( physSession == null )
    {
      _logger.LogError( $"Unable to get session, sessionId '{sessionId}' not found" );
      return null;
    }

    return physSession;
  }
}
