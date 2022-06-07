using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.ObjectMapper
{
  public class Files : ObjectMapper<SystemFiles, FilesDto>
  {
    public Files(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public Files(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override FilesDto PhysicalToDto(SystemFiles phys, Object source = null)
    {
      var dto = GetDto( source );

      dto.CreatedAt = phys.CreatedAt;
      dto.Description = phys.Description;
      dto.Id = phys.Id;
      dto.Name = phys.Name;
      dto.UpdatedAt = phys.UpdatedAt;

      return dto;
    }

    public override SystemFiles DtoToPhysical(FilesDto dto, Object source = null)
    {
      var phys = GetPhys( source );

      phys.Id = (uint)dto.Id;
      phys.CreatedAt = dto.CreatedAt;
      phys.Description = dto.Description;
      phys.Name = dto.Name;
      phys.UpdatedAt = dto.UpdatedAt;

      return phys;
    }

    public override SystemFiles ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      SystemFiles phys = GetPhys(source);

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "map_id").Value, out uint id))
        phys.ImageableId = id;
      phys.Mime = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "mime"));

      phys.Name = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "name"));
      phys.Path = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "path"));
      phys.Args = elements.FirstOrDefault(x => x.Name == "args").Value;

      phys.Width = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "width").Value);
      phys.WidthType = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "width_type"));
      phys.Height = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "height").Value);
      phys.HeightType = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "height_type"));

      phys.HAlign = elements.FirstOrDefault(x => x.Name == "h_align").Value;
      phys.VAlign = elements.FirstOrDefault(x => x.Name == "v_align").Value;

      phys.IsShared = Convert.ToSByte(elements.FirstOrDefault(x => x.Name == "is_shared").Value);
      phys.IsPrivate = Convert.ToSByte(elements.FirstOrDefault(x => x.Name == "is_private").Value);
      phys.CreatedAt = DateTime.Now;

      // logger.LogInformation($"loaded SystemFiles {phys.Id}");

      return phys;
    }

  }
}