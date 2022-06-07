using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class Questions : OLabMapper<SystemQuestions, QuestionsDto>
  {
    public Questions(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override SystemQuestions ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      SystemQuestions phys = GetPhys( source );

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      CreateIdTranslation(phys.Id);
      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "map_id").Value, out uint id))
        phys.ImageableId = id;

      phys.ImageableType = "Maps";
      phys.Stem = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "stem"));

      phys.EntryTypeId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "entry_type_id").Value);
      phys.Width = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "width").Value);
      phys.Height = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "height").Value);
      phys.Feedback = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "feedback"));
      phys.Prompt = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "prompt"));

      phys.ShowAnswer = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "show_answer").Value) == 1 ? true : false;
      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "counter_id").Value, out uint id1))
        phys.CounterId = id1;
      if (int.TryParse(elements.FirstOrDefault(x => x.Name == "num_tries").Value, out int id2))
        phys.NumTries = id2;
      phys.ShowSubmit = Convert.ToSByte(elements.FirstOrDefault(x => x.Name == "show_submit").Value);
      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "redirect_node_id").Value, out uint id3))
        phys.RedirectNodeId = id3;
      phys.SubmitText = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "submit_text"));

      phys.TypeDisplay = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "type_display").Value);
      phys.Settings = Conversions.Base64Decode(elements.FirstOrDefault(x => x.Name == "settings"));

      phys.IsPrivate = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "is_private").Value);

      if (int.TryParse(elements.FirstOrDefault(x => x.Name == "order").Value, out int id4))
        phys.Order = id4;

      if (uint.TryParse(elements.FirstOrDefault(x => x.Name == "external_source_id").Value, out uint id5))
        phys.ExternalSourceId = id5.ToString();
      phys.CreatedAt = DateTime.Now;

      // logger.LogInformation($"loaded SystemQuestions {phys.Id}");

      return phys;
    }

  }
}