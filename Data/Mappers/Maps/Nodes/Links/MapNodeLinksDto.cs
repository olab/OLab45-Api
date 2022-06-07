using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class MapNodeLinksMapper : ObjectMapper<MapNodeLinks, MapNodeLinksDto>
  {
    private static Random random = null;

    public MapNodeLinksMapper(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public MapNodeLinksMapper(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
      if (random == null)
        random = new Random((int)DateTime.Now.Ticks);
    }

    public new IList<MapNodeLinksDto> PhysicalToDto(IList<MapNodeLinks> physList)
    {
      var dtoList = new List<MapNodeLinksDto>();

      foreach (var phys in physList.OrderBy(x => x.Order))
      {
        // do isHidden check
        if (phys.Hidden.HasValue && phys.Hidden.Value)
          continue;

        // do a probability of showing check
        if (phys.Probability.HasValue && (phys.Probability.Value > 0))
        {
          var chance = random.Next() % 100;
          if (chance > phys.Probability)
            continue;
        }

        var dto = new MapNodeLinksDto();
        PhysicalToDto(phys, dto);
        dtoList.Add(dto);
      }

      // hook up two-way links
      foreach (var itemOuter in dtoList)
      {
        foreach (var itemInner in dtoList)
        {
          if ((itemOuter.SourceId == itemInner.DestinationId) && (itemOuter.DestinationId == itemInner.SourceId))
            itemOuter.ReverseId = itemInner.Id;
        }

      }

      return dtoList;
    }

    internal static void InternalPhysicalToDto(MapNodeLinks phys, MapNodeLinksDto dto)
    {
      dto.Color = phys.Color;
      dto.DestinationId = phys.NodeId2;
      dto.SourceId = phys.NodeId1;
      dto.Id = Conversions.OptionalIdSafeAssign(phys.Id);
      dto.LinkStyleId = phys.LinkStyleId;
      dto.LinkText = phys.Text;
    }

    public override MapNodeLinksDto PhysicalToDto(MapNodeLinks phys, Object source = null)
    {
      var dto = GetDto(source);
      InternalPhysicalToDto(phys, dto);
      return dto;
    }

    internal static void InternalDtoToPhysical(MapNodeLinksDto dto, MapNodeLinks phys)
    {
      phys.Color = dto.Color;
      phys.NodeId2 = ( uint )dto.DestinationId;
      phys.NodeId1 = ( uint )dto.SourceId;
      phys.Id = ( uint )Conversions.OptionalIdSafeAssign(dto.Id);
      phys.LinkStyleId = phys.LinkStyleId;
      phys.Text = dto.LinkText;
    }

    public override MapNodeLinks DtoToPhysical(MapNodeLinksDto dto, Object source = null)
    {
      var phys = GetPhys(source);
      InternalDtoToPhysical(dto, phys);
      return phys;
    }

    public override MapNodeLinks ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      var phys = GetPhys(source);

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      CreateIdTranslation(phys.Id);
      phys.MapId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "map_id").Value);
      phys.NodeId1 = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "node_id_1").Value);
      phys.NodeId2 = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "node_id_2").Value);
      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "image_id").Value, out uint uiTemp))
        phys.ImageId = uiTemp;
      phys.Text = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "text"));
      phys.Order = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "order").Value);
      phys.Probability = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "probability").Value);
      if (int.TryParse(elements.FirstOrDefault(x => x.Name == "hidden").Value, out int nTemp))
        phys.Hidden = ( nTemp == 1 );
      phys.CreatedAt = DateTime.Now;

      // logger.LogInformation($"loaded MapNodeLinks {phys.Id}");

      return phys;
    }

  }
}
