using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
    /// <summary>
    /// Defines a Atrium Update command method
    /// </summary>
    public class LearnerAssignmentCommand : CommandMethod
    {
        public Participant Data { get; set; }

        public LearnerAssignmentCommand(Participant moderator, Participant learner) : base(moderator.CommandChannel, "learnerassignment")
        {
            Data = learner;
        }
        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}