using System;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.ObjectMapper
{
  public class ScopedObjects : ObjectMapper<Model.ScopedObjects, ScopedObjectsDto>
  {
    protected readonly bool enableWikiTranslation = true;

    public ScopedObjects(OLabLogger logger, bool enableWikiTranslation = true) : base( logger )
    {
    }

    public ScopedObjects(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override ScopedObjectsDto PhysicalToDto(Model.ScopedObjects phys, object source = null)
    {
      var dto = GetDto(source);

      var dtoQuestionsList = new ObjectMapper.QuestionsFull(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Questions);
      dto.Questions.AddRange(dtoQuestionsList);

      var dtCountersList = new ObjectMapper.Counters(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Counters);
      dto.Counters.AddRange(dtCountersList);

      var dtConstantsList = new ObjectMapper.Constants(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Constants);
      dto.Constants.AddRange(dtConstantsList);

      var dtFilesList = new ObjectMapper.FilesFull(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Files);
      dto.Files.AddRange(dtFilesList);

      var dtScriptsList = new ObjectMapper.Scripts(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Scripts);
      dto.Scripts.AddRange(dtScriptsList);

      var dtThemesList = new ThemesFull(logger, GetWikiProvider(), enableWikiTranslation).PhysicalToDto(phys.Themes);
      dto.Themes.AddRange(dtThemesList);

      var dtCounterActionsList = new CounterActionsMapper(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.CounterActions);
      dto.CounterActions.AddRange(dtCounterActionsList);

      return dto;
    }

    public override Model.ScopedObjects DtoToPhysical(ScopedObjectsDto dto, object source = null)
    {
      throw new NotImplementedException();
    }

  }
}