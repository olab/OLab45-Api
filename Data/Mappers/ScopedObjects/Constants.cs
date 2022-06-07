using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using System.Text;

namespace OLabWebAPI.ObjectMapper
{
  public class Constants : OLabMapper<SystemConstants, ConstantsDto>
  {
    public Constants(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public Constants(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base(logger, tagProvider)
    {
    }

    public override ConstantsDto PhysicalToDto(SystemConstants phys, ConstantsDto dto)
    {
      dto.Value = Encoding.ASCII.GetString(phys.Value);
      return dto;
    }

    public override SystemConstants DtoToPhysical(ConstantsDto dto, SystemConstants phys)
    {
      phys.Value = Encoding.ASCII.GetBytes(dto.Value);
      return phys;
    }

    /// <summary>
    /// Create a physical object from an element array
    /// </summary>
    /// <param name="elements">List of elements</param>
    /// <param name="source">Physical object</param>
    /// <returns>Physical object</returns>
    public override SystemConstants ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      SystemConstants phys = GetPhys(source);

      phys.Id = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      CreateIdTranslation((uint)phys.Id);

      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "map_id").Value, out uint id))
        phys.ImageableId = id;
      phys.ImageableType = Utils.Constants.ScopeLevelMap;

      var value = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "mime"));
      phys.Name = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "name"));

      value = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "path"));

      phys.CreatedAt = DateTime.Now;

      return phys;
    }
  }
}