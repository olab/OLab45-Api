using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class ModeratorRemovedCommand : CommandMethod
  {
    /// <summary>
    /// Defines a Moderator removed command method
    /// </summary>
    public ModeratorRemovedCommand(string groupName) : base(groupName, "moderatorremoved")
    {
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}