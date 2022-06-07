using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;
using AutoMapper;

namespace OLabWebAPI.ObjectMapper
{
  public abstract class OLabMapper<P, D> : object where P : new() where D : new()
  {
    protected readonly Mapper _mapper;

    protected OLabLogger logger;
    protected WikiTagProvider _tagProvider = null;
    // used to hold on to id translation between origin system and new one
    protected IDictionary<uint, uint?> _idTranslation = new Dictionary<uint, uint?>();

    public virtual P ElementsToPhys(IEnumerable<dynamic> elements, Object source = null) { return default; }
    public WikiTagProvider GetWikiProvider() { return _tagProvider; }

    public OLabMapper(OLabLogger logger)
    {
      this.logger = logger;
      _tagProvider = new WikiTagProvider(logger);
      _mapper = new Mapper(GetConfiguration());
    }

    public OLabMapper(OLabLogger logger, WikiTagProvider tagProvider)
    {
      this.logger = logger;
      _tagProvider = tagProvider;
      _mapper = new Mapper(GetConfiguration());
    }

    /// <summary>
    /// Default (overridable) AutoMapper configuration
    /// </summary>
    /// <returns>MapperConfiguration</returns>
    protected virtual MapperConfiguration GetConfiguration()
    {
      return new MapperConfiguration(cfg =>
       cfg.CreateMap<P, D>()
       .ReverseMap()
      );
    }

    /// <summary>
    /// Convert a physical object to a specific dto. 
    /// </summary>
    /// <remarks>
    /// Allows for derived class specific overrides that 
    /// don't fit well with default implementation
    /// </remarks>
    /// <param name="phys">Physical object</param>
    /// <param name="source">Base dto object</param>
    /// <returns>Dto object</returns>
    public virtual D PhysicalToDto(P phys, D source)
    {
      return source;
    }

    /// <summary>
    /// Convert a physical object to new dto. 
    /// </summary>
    /// <remarks>
    /// Allows for derived class specific overrides that 
    /// don't fit well with default implementation
    /// </remarks>
    /// <param name="phys">Physical object</param>
    /// <returns>Dto object</returns>
    public virtual D PhysicalToDto(P phys)
    {
      var dto = _mapper.Map<D>(phys);
      dto = PhysicalToDto(phys, dto);
      return dto;
    }

    /// <summary>
    /// Convert a list of physical objects to Dto
    /// </summary>
    /// <param name="physList">Physical objects</param>
    /// <returns></returns>
    public virtual IList<D> PhysicalToDto(IList<P> physList)
    {
      var dtoList = new List<D>();
      foreach (var phys in physList)
      {
        var dto = PhysicalToDto(phys);
        dtoList.Add(dto);
      }

      return dtoList;
    }

    /// <summary>
    /// Convert a dto object to a specific physicla one. 
    /// </summary>
    /// <remarks>
    /// Allows for derived class specific overrides that 
    /// don't fit well with default implementation
    /// </remarks>
    /// <param name="dto">Ddto object</param>
    /// <param name="source">Base physical object</param>
    /// <returns>Physical object</returns>
    public virtual P DtoToPhysical(D dto, P source)
    {
      return source;
    }

    /// <summary>
    /// Convert a dto object to new physical one. 
    /// </summary>
    /// <remarks>
    /// Allows for derived class specific overrides that 
    /// don't fit well with default implementation
    /// </remarks>
    /// <param name="dto">Dto object</param>
    /// <returns>Physical object</returns>
    public virtual P DtoToPhysical(D dto)
    {
      var phys = new P();
      _mapper.Map(dto, phys);

      phys = DtoToPhysical(dto, phys);
      return phys;
    }

    /// <summary>
    /// Convert a list of dto objects to physical
    /// </summary>
    /// <param name="dtoList">Dto objects</param>
    /// <returns></returns>
    public virtual IList<P> DtoToPhysical(IList<D> dtoList)
    {
      var physList = new List<P>();
      foreach (var dto in dtoList)
      {
        var phys = DtoToPhysical(dto);
        physList.Add(phys);
      }

      return physList;
    }

    /// <summary>
    /// Convert an Object to a Dto
    /// </summary>
    /// <param name="source"></param>
    /// <returns>Dto</returns>
    public virtual D GetDto(Object source = null)
    {
      if (source == null)
        return new D();

      return (D)source;
    }

    /// <summary>
    /// Convert an Object to a Physical
    /// </summary>
    /// <param name="source"></param>
    /// <returns>Physical</returns>
    public virtual P GetPhys(Object source = null)
    {
      if (source == null)
        return new P();

      return (P)source;
    }

    protected void CreateIdTranslation(uint originalId)
    {
      if (_idTranslation.ContainsKey(originalId))
        return;
      _idTranslation.Add(originalId, null);
    }

    protected bool SetIdTranslation(uint originalId, uint newId)
    {
      return _idTranslation.TryAdd(originalId, newId);
    }

    protected uint? GetIdTranslation(uint originalId)
    {
      if (!_idTranslation.TryGetValue(originalId, out uint? newId))
        return newId;

      throw new KeyNotFoundException($"Cound not find Id key {originalId}");
    }

  }
}