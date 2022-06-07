using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class Scripts : OLabMapper<SystemScripts, ScriptsDto>
  {
    public Scripts(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {        
    }    
  }
}