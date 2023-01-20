using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OLabWebAPI.Services.TurkTalk.Contracts
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
            return JValue.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}