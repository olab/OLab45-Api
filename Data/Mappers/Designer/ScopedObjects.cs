using System;
using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Dto.Designer;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.ObjectMapper.Designer
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

      var dtConstantsList = new Constants(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Constants);
      dto.Constants.AddRange(dtConstantsList);

      var dtoQuestionsList = new Questions(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Questions);
      dto.Questions.AddRange(dtoQuestionsList);

      var dtCountersList = new Counters(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Counters);
      dto.Counters.AddRange(dtCountersList);

      var dtFilesList = new Files(logger, new WikiTagProvider(logger)).PhysicalToDto(phys.Files);
      dto.Files.AddRange(dtFilesList);

      return dto;
    }

    public override Model.ScopedObjects DtoToPhysical(ScopedObjectsDto dto, object source = null)
    {
      throw new NotImplementedException();
    }

  }
}