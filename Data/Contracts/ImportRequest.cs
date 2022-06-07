using System.ComponentModel.DataAnnotations;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Model
{
    public class ImportRequest
    {
        public ImportRequest()
        {
            MessageLevel = OLabLogMessage.MessageLevel.Info;
        }
        
        [Required]
        public string FileName { get; set; }
        public OLabLogMessage.MessageLevel MessageLevel { get; set; }
    }
}