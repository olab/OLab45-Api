using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class ModeratorJoinedCommand : CommandMethod
  {
    /// <summary>
    /// Defined a Moderator Joined command method
    /// </summary>
    public string ModeratorName { get; set; }
    public ModeratorJoinedCommand(string groupName, string moderatorName) : base(groupName, "moderatorjoined")
    {
      ModeratorName = ModeratorName;
    }

  }
}