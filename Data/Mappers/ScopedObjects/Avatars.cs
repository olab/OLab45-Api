using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class AvatarsMapper : OLabMapper<MapAvatars, AvatarsDto>
  {
    public AvatarsMapper(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override MapAvatars ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      MapAvatars phys = GetPhys(source);

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      CreateIdTranslation((uint)phys.Id);

      phys.MapId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "map_id").Value);
      phys.Skin1 = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "skin_1"));
      phys.Skin2 = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "skin_2"));
      // phys.Cloth = elements.FirstOrDefault(x => x.Name == "cloth").Value;
      phys.Nose = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "nose"));
      phys.Hair = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "hair"));
      phys.Environment = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "environment"));
      phys.Accessory1 = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "accessory_1"));
      phys.Bkd = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "bkd"));
      phys.Sex = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "sex"));
      phys.Mouth = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "mouth"));
      phys.Outfit = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "outfit"));
      phys.Bubble = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "bubble"));
      phys.BubbleText = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "bubble_text"));
      phys.Accessory2 = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "accessory_2"));
      phys.Accessory3 = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "accessory_3"));
      phys.Age = elements.FirstOrDefault(x => x.Name == "age").Value;
      phys.Eyes = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "eyes"));
      // phys.HairColor = elements.FirstOrDefault(x => x.Name == "hair_color").Value;
      phys.Image = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "image"));

      phys.IsPrivate = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "is_private").Value);

      // logger.LogInformation($"loaded MapAvatars {phys.Id}");

      return phys;
    }

  }
}