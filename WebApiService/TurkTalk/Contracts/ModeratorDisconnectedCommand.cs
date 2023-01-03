using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class ModeratorDisconnectedCommand : CommandMethod
  {
    /// <summary>
    /// Defined a Moderator Joined command method
    /// </summary>
    public string ModeratorName { get; set; }
    public ModeratorDisconnectedCommand(string groupName) : base(groupName, "moderatordisconnected")
    {
      ModeratorName = ModeratorName;
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}