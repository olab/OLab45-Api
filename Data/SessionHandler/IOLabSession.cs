using System;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Data.Session
{
  public interface IOLabSession
  {
    public void OnStartSession(string userName, uint mapId, string ipAddress );
    public void OnPlayNode(string sessionId, uint mapId, uint nodeId);
    public void OnQuestionResponse(string sessionId, uint mapId, uint nodeId, uint questionId, string value);
    public void OnEndSession(string sessionId, uint mapId, uint nodeId);
    public string GetSessionId();
    
    public static string GenerateSessionId()
    {
      var sessionId = Guid.NewGuid().ToString();
      return sessionId;
    }
  }
}