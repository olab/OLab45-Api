using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
    /// <summary>
    /// Defines a Message method
    /// </summary>
    public class MessageMethod : Method
    {
        public string Data { get; set; }
        public string FromName { get; set; }

        public MessageMethod(string groupName, string fromName, string messageText) : base(groupName, "message")
        {
            Data = messageText;
            FromName = fromName;
        }

        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}