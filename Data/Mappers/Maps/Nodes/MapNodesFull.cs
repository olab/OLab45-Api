using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class MapNodesFullMapper : OLabMapper<Model.MapNodes, MapNodesFullDto>
  {
    protected readonly bool enableWikiTranslation = false;

    public MapNodesFullMapper(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
      this.enableWikiTranslation = enableWikiTranslation;
    }

    public MapNodesFullMapper(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
      this.enableWikiTranslation = enableWikiTranslation;
    }

    public override MapNodesFullDto PhysicalToDto(Model.MapNodes phys, MapNodesFullDto dto)
    {
      dto.Height = phys.Height.HasValue ? phys.Height : ObjectMapper.MapNodesMapper.DefaultHeight;

      if (enableWikiTranslation)
        dto.Text = _tagProvider.Translate(phys.Text);
      else
        dto.Text = phys.Text;
      dto.Width = phys.Width.HasValue ? phys.Width : ObjectMapper.MapNodesMapper.DefaultWidth;

      dto.Width = phys.Width.HasValue ? phys.Width : ObjectMapper.MapNodesMapper.DefaultWidth;

      return dto;
    }
  }
}
