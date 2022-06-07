using System;
using System.Linq;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;
using AutoMapper;

namespace OLabWebAPI.ObjectMapper
{
  public class CounterActionsMapper : OLabMapper<SystemCounterActions, CounterActionsDto>
  {
    public static int DefaultWidth = 400;
    public static int DefaultHeight = 300;

    public CounterActionsMapper(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base(logger, tagProvider)
    {
    }

    /// <summary>
    /// Default (overridable) AutoMapper configuration
    /// </summary>
    /// <returns>MapperConfiguration</returns>
    protected override MapperConfiguration GetConfiguration()
    {
      return new MapperConfiguration(cfg =>
       cfg.CreateMap<SystemCounterActions, CounterActionsDto>()
        .ForMember(dest => dest.NodeId, act => act.MapFrom(src => src.ImageableId))
        .ForMember(dest => dest.Function, act => act.MapFrom(src => src.Expression))
        .ForMember(dest => dest.Display, act => act.MapFrom(src => src.Visible))
        .ReverseMap()
      );
    }

    public override SystemCounterActions ElementsToPhys(IEnumerable<dynamic> elements, Object source = null)
    {
      var phys = GetPhys(source);

      phys.Id = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "id").Value);
      phys.OperationType = "open";
      phys.ImageableId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "node_id").Value);
      phys.ImageableType = Utils.Constants.ScopeLevelNode;
      phys.CounterId = Convert.ToUInt32(elements.FirstOrDefault(x => x.Name == "counter_id").Value);
      phys.Expression = elements.FirstOrDefault(x => x.Name == "function").Value;
      phys.Visible = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "display").Value);

      return phys;
    }

  }
}
