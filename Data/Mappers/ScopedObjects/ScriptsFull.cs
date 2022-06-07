using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class ScriptsFull : OLabMapper<SystemScripts, ScriptsFullDto>
  {
    public ScriptsFull(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {        
    }
  }
}