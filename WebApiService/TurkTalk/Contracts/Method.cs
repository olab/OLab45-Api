using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public abstract class Method
  {
    public string MethodName { get; set; }
    public string CommandChannel { get; set; }

    public Method(string recipientGroupName, string methodName)
    {
      Guard.Argument(recipientGroupName).NotEmpty(recipientGroupName);
      Guard.Argument(methodName).NotEmpty(methodName);

      MethodName = methodName;
      CommandChannel = recipientGroupName;
    }

    public virtual string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JValue.Parse(rawJson).ToString(Formatting.Indented);
    }
  }
}