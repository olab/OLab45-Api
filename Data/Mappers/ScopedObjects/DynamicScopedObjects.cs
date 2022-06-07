using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.ObjectMapper
{
  public class DynamicScopedObjects : ObjectMapper<Model.ScopedObjects, DynamicScopedObjectsDto>
  {
    protected readonly bool enableWikiTranslation = true;

    public DynamicScopedObjects(OLabLogger logger, bool enableWikiTranslation = true) : base(logger)
    {
    }

    public DynamicScopedObjects(OLabLogger logger, WikiTagProvider tagProvider, bool enableWikiTranslation = true) : base( logger, tagProvider )
    {
    }

    public override DynamicScopedObjectsDto PhysicalToDto(Model.ScopedObjects phys, object source = null)
    {
      throw new System.NotImplementedException();
    }  

    public override Model.ScopedObjects DtoToPhysical(DynamicScopedObjectsDto dto, object source = null)
    {
      throw new System.NotImplementedException();
    } 

    public DynamicScopedObjectsDto PhysicalToDto(
      Model.ScopedObjects server,
      Model.ScopedObjects map,
      Model.ScopedObjects node
    )
    {
      var dto = new DynamicScopedObjectsDto();
      PhysicalToDto(server, map, node, dto);
      return dto;
    }

    public void PhysicalToDto(
      Model.ScopedObjects server,
      Model.ScopedObjects map,
      Model.ScopedObjects node, 
      DynamicScopedObjectsDto dto)
    {
      var dtoCountersList = new ObjectMapper.Counters(logger, new WikiTagProvider(logger)).PhysicalToDto( server.Counters );
      dto.Server.Counters.AddRange(dtoCountersList);

      dtoCountersList = new ObjectMapper.Counters(logger, new WikiTagProvider(logger)).PhysicalToDto( map.Counters );
      dto.Map.Counters.AddRange(dtoCountersList);

      dtoCountersList = new ObjectMapper.Counters(logger, new WikiTagProvider(logger)).PhysicalToDto( node.Counters );
      dto.Node.Counters.AddRange(dtoCountersList);

      // var dtoConstantsList = new ConstantsObjectMapper(logger, new WikiTagProvider(logger)).PhysicalToDto( server.Constants );
      // dto.Server.Constants.AddRange(dtoConstantsList);

    }

  }
}