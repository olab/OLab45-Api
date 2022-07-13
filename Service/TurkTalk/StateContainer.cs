using System;
using System.Collections.Generic;
using TurkTalk.Contracts;

namespace OLabWebAPI.Services
{
  public static class StateContainer
  {
    public static Dictionary<string, Participant> Attendees = new Dictionary<string, Participant>();
    public static Dictionary<string, Participant> Moderators = new Dictionary<string, Participant>();

    public static Participant SuperModerator = new Participant("Moderator", "");

    public static string SuperModeratorName
    {
      get { return SuperModerator.Name; }
    }

    public static string SuperModeratorConnectionId
    {
      get { return SuperModerator.SessionId; }
      set { SuperModerator.SessionId = value; }
    }

    public static bool IsSuperModeratorConnected { 
      get { return !string.IsNullOrWhiteSpace(SuperModerator.SessionId); } 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public static bool DisconnectSession(string connectionId)
    {
      bool found = Attendees.TryGetValue(connectionId, out Participant item);
      if (found)
        item.InChat = false;
      return found;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public static bool DisconnectAttendee(string connectionId)
    {
      bool found = Attendees.Remove(connectionId);
      return found;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void DisconnectModerator()
    {
      SuperModerator.SessionId = "";
      foreach (var item in Attendees.Values)
        item.InChat = false;
    }
  }

}