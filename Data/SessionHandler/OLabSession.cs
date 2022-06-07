using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI.Data.Session
{
  public class OLabSession : IOLabSession
  {
    // private readonly AppSettings _appSettings;
    private readonly OLabDBContext _context;
    private readonly ILogger _logger;
    private string _sessionId;

    public OLabSession(ILogger logger, OLabDBContext context)
    {
      // _appSettings = appSettings.Value;
      _context = context;
      _logger = logger;
    }

    public string GetSessionId()
    {
      return _sessionId;
    }

    public void OnStartSession(string userName, uint mapId, string ipAddress)
    {
      _sessionId = IOLabSession.GenerateSessionId();

      var session = new UserSessions
      {
        Uuid = _sessionId,
        MapId = mapId,
        StartTime = GetUnixTime(),
        UserIp = ipAddress
      };

      _context.UserSessions.Add(session);
      _context.SaveChanges();

      _logger.LogInformation($"OnStartSession: session {_sessionId} ({userName}) MapId: {mapId}. Session PK: {session.Id}");
    }

    public void OnEndSession(string sessionId, uint mapId, uint nodeId)
    {
      _logger.LogInformation($"OnEndSession: session {sessionId} Map: {mapId} Node: {nodeId}");

      var session = GetSession(sessionId);
      if (session == null)
        return;

      session.EndTime = GetUnixTime();

      _context.UserSessions.Update(session);
      _context.SaveChanges();
    }

    public void OnPlayNode(string sessionId, uint mapId, uint nodeId)
    {
      _logger.LogInformation($"OnPlayNode: session {sessionId} Map: {mapId} Node: {nodeId}");

      var session = GetSession(sessionId);
      if (session == null)
        return;

      var sessionTrace = new UserSessionTraces
      {
        SessionId = session.Id,
        MapId = mapId,
        NodeId = nodeId,
        DateStamp = GetUnixTime()
      };

      _context.UserSessionTraces.Add(sessionTrace);
      _context.SaveChanges();

    }

    public void OnQuestionResponse(string sessionId, uint mapId, uint nodeId, uint questionId, string value)
    {
      _logger.LogInformation($"OnQuestionResponse: session {sessionId} Map: {mapId} Node: {nodeId} Question: {questionId} = {value} ");

      var session = GetSession(sessionId);
      if (session == null)
        return;

      var userResponse = new UserResponses
      {
        SessionId = session.Id,
        QuestionId = questionId,
        Response = value,
        NodeId = nodeId,
        CreatedAt = GetUnixTime()
      };

      _context.UserResponses.Add(userResponse);
      _context.SaveChanges();
    }

    private decimal GetUnixTime()
    {
      TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      double unixTime = span.TotalSeconds;
      return (decimal)unixTime;
    }

    private UserSessions GetSession(string sessionId)
    {
      var session = _context.UserSessions.Where(x => x.Uuid == sessionId).FirstOrDefault();
      if (session == null)
      {
        _logger.LogError($"Unable to get session, sessionId '{sessionId}' not found");
        return null;
      }

      return session;
    }
  }
}