using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Text;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.ObjectMapper
{
  public class Counters : OLabMapper<SystemCounters, CountersDto>
  {
    public Counters(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public Counters(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override CountersDto PhysicalToDto(SystemCounters phys, CountersDto dto)
    {
      if (phys.Value != null)
        dto.Value = Encoding.ASCII.GetString(phys.Value);
      if ( dto.Value == null )
        dto.Value = "";
      return dto;
    }

    public override SystemCounters DtoToPhysical(CountersDto dto, SystemCounters phys)
    {
      phys.Value = Encoding.ASCII.GetBytes(dto.Value);    
      return phys;
    }  

    public override SystemCounters ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      SystemCounters phys = GetPhys(source);

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      phys.Name = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "name"));
      phys.Description = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "description"));
      phys.StartValue = Encoding.ASCII.GetBytes(elements.FirstOrDefault(x => x.Name == "start_value").Value);
      phys.CreatedAt = DateTime.Now;

      // logger.LogInformation($"loaded SystemCounters {phys.Id}");

      return phys;
    }

  }
}