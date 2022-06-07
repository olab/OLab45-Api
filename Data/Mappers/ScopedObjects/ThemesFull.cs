using System;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Utils;
using Newtonsoft.Json;
using OLabWebAPI.Common;

namespace OLabWebAPI.ObjectMapper
{
  public class ThemesFull : OLabMapper<SystemThemes, ThemesFullDto>
  {
    protected readonly bool enableWikiTranslation;

    public ThemesFull(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
      this.enableWikiTranslation = enableWikiTranslation;
    }

    public override ThemesFullDto PhysicalToDto(SystemThemes phys, ThemesFullDto dto)
    {
      if (enableWikiTranslation)
      {
        dto.HeaderText = _tagProvider.Translate(phys.HeaderText);
        dto.FooterText = _tagProvider.Translate(phys.FooterText);
      }
      else
      {
        dto.HeaderText = phys.HeaderText;
        dto.FooterText = phys.FooterText;
      }
      return dto;
    }

    [JsonProperty("header")]
    public string HeaderText { get; set; }
    [JsonProperty("footer")]
    public string FooterText { get; set; }
  }
}