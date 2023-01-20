using OLabWebAPI.Services.TurkTalk.Contracts;
using System.Collections.Generic;

namespace OLabWebAPI.TurkTalk.Contracts
{
    public class ModeratorAssignmentCommand : CommandMethod
    {
        public ModeratorAssignmentPayload Data { get; set; }

        public Moderator Remote { get; set; }

        public ModeratorAssignmentCommand(Moderator remote, IList<MapNodeListItem> mapNodes) : base(remote.CommandChannel, "moderatorassignment")
        {
            Data = new ModeratorAssignmentPayload { Remote = remote, MapNodes = mapNodes };
        }
    }
}
