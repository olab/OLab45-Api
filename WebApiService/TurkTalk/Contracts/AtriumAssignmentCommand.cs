using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
    /// <summary>
    /// Defines a Atrium Update command method
    /// </summary>
    public class AtriumAssignmentCommand : CommandMethod
    {
        public Learner Data { get; set; }

        public AtriumAssignmentCommand(Participant participant, Learner atriumParticipant)
          : base(participant.CommandChannel, "atriumassignment")
        {
            Data = atriumParticipant;
        }

        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}