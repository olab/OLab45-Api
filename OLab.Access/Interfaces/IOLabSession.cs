using OLab.Api.Dto;
using OLab.Api.Model;
using System;

namespace OLab.Access.Interfaces;

public interface IOLabSession
{
  public void SetMapId(uint mapId);
  public void OnStartSession();
  public void OnPlayNode(MapsNodesFullRelationsDto dto);
  public void OnQuestionResponse(
    QuestionResponsePostDataDto body,
    SystemQuestions questionPhys);
  public void OnExtendSessionEnd(uint nodeId);
  public string GetSessionId();
  public void SetSessionId(string sessionId);
  public void SaveSessionState(
    uint nodeId,
    DynamicScopedObjectsDto dynamicObjects);

  public static string GenerateSessionId()
  {
    var sessionId = Guid.NewGuid().ToString();
    return sessionId;
  }
}