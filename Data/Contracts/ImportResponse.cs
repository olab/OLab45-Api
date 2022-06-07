using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Model
{
    public class ImportResponse
    {
        public IList<OLabLogMessage> Messages { get; set; }

        public ImportResponse()
        {
            Messages = new List<OLabLogMessage>();
        }
    }
}