using OLabWebAPI.Utils;
using OLabWebAPI.Dto;
using OLabWebAPI.Common;
using AutoMapper;

namespace OLabWebAPI.ObjectMapper
{
  public class MapNodeLinksFullMapper : OLabMapper<Model.MapNodeLinks, MapNodeLinksFullDto>
  {
    public MapNodeLinksFullMapper(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public MapNodeLinksFullMapper(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base(logger, tagProvider)
    {
    }

    /// <summary>
    /// Default (overridable) AutoMapper configuration
    /// </summary>
    /// <returns>MapperConfiguration</returns>
    protected override MapperConfiguration GetConfiguration()
    {
      return new MapperConfiguration(cfg =>
       cfg.CreateMap<Model.MapNodeLinks, Dto.MapNodeLinksFullDto>()
        .ForMember(dest => dest.SourceId, act => act.MapFrom(src => src.NodeId1))
        .ForMember(dest => dest.DestinationId, act => act.MapFrom(src => src.NodeId2))
        .ReverseMap()
      );
    }    

  }
}
