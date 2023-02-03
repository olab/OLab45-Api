using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
    /// <summary>
    /// Defines a command to remove a connection from a room
    /// </summary>
    public class RoomUnassignmentCommand : CommandMethod
    {
        public Participant Data { get; set; }

        public RoomUnassignmentCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "learnerunassignment")
        {
            Guard.Argument(participant).NotNull(nameof(participant));
            Data = participant;
        }

        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}