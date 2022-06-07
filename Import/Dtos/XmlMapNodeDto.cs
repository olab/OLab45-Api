using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{

  public class XmlMapNodeDto : XmlImportDto<XmlMapNodes>
  {
    private readonly ObjectMapper.MapNodesMapper _mapper;

    public XmlMapNodeDto(Importer importer) : base(importer, "map_node.xml")
    {
      _mapper = new ObjectMapper.MapNodesMapper(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.map_node.Elements();
    }

    /// <summary>
    /// Replaces (deprecated) VPD tags with CONST
    /// </summary>
    /// <param name="item">Source MapNode</param>
    /// <returns>true if found replacement</returns>
    public bool ReplaceVpdWikiTags(Model.MapNodes item)
    {
      bool rc = false;

      // replace all VPD with CONST in node text
      var vpdDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapVpdElementDto) as XmlMapVpdDto;

      var wiki = new VpdWikiTag(GetLogger());
      while (wiki.HaveWikiTag(item.Text))
      {
        var id = Convert.ToUInt32(wiki.GetWikiArgument1());
        var wikiTag = wiki.GetWikiType();

        var newId = vpdDto.GetIdTranslation(GetFileName(), id);

        var newWikiTag = wikiTag.Replace(id.ToString(), newId.ToString());
        newWikiTag = newWikiTag.Replace(wiki.GetWikiType(), "CONST");

        GetLogger().LogDebug($"Replacing '{wikiTag}' -> '{newWikiTag}'");
        item.Text = item.Text.Replace(wikiTag, newWikiTag);

        rc = true;
      }

      return rc;
    }

    /// <summary>
    /// Replaces (deprecated) Avatar tags with CONST
    /// </summary>
    /// <param name="item">Source MapNode</param>
    /// <returns>true if found replacement</returns>
    public bool ReplaceAvWikiTags(Model.MapNodes item)
    {
      bool rc = false;

      // replace all VPD with CONST in node text
      var avDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapAvatarDto) as XmlMapAvatarDto;

      var wiki = new AvatarWikiTag(GetLogger());
      while (wiki.HaveWikiTag(item.Text))
      {
        var id = Convert.ToUInt16(wiki.GetWikiArgument1());
        var wikiTag = wiki.GetWikiType();

        var newId = avDto.GetIdTranslation(GetFileName(), id);

        var newWikiTag = wikiTag.Replace(id.ToString(), newId.ToString());
        newWikiTag = newWikiTag.Replace(wiki.GetWikiType(), "MR");

        GetLogger().LogDebug($"Replacing '{wikiTag}' -> '{newWikiTag}'");
        item.Text = item.Text.Replace(wikiTag, newWikiTag);

        rc = true;
      }

      return rc;
    }

    public bool RemapMrWikiTags(Model.MapNodes item)
    {
      bool rc = true;

      // remap all MR with new id's in node text
      var mapElementDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapElementDto);
      var mappedWikiTags = new Dictionary<string, string>();

      var wiki = new MediaResourceWikiTag(GetLogger());
      while (wiki.HaveWikiTag(item.Text))
      {
        var id = Convert.ToUInt32(wiki.GetWikiArgument1());
        var wikiTag = wiki.GetWikiType();

        var newId = mapElementDto.GetIdTranslation(GetFileName(), id);

        var newWikiTag = wikiTag.Replace(id.ToString(), newId.ToString());
        var tmpWikiTag = wikiTag.Replace(wiki.GetWikiType(), "temp");

        item.Text = item.Text.Replace(wikiTag, tmpWikiTag);

        mappedWikiTags.Add(tmpWikiTag, newWikiTag);
      }

      foreach (var key in mappedWikiTags.Keys)
      {
        GetLogger().LogDebug($"Remapping '{key}' -> '{mappedWikiTags[key]}'");
        item.Text = item.Text.Replace(key, mappedWikiTags[key]);
      }

      return rc;
    }

    public bool RemapQrWikiTags(Model.MapNodes item)
    {
      bool rc = true;

      // remap all MR with new id's in node text
      var questionDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapQuestionDto);
      var mappedWikiTags = new Dictionary<string, string>();

      var wiki = new QuestionWikiTag(GetLogger());
      while (wiki.HaveWikiTag(item.Text))
      {
        var id = Convert.ToUInt32(wiki.GetWikiArgument1());
        var wikiTag = wiki.GetWikiType();

        var newId = questionDto.GetIdTranslation(GetFileName(), id);

        var newWikiTag = wikiTag.Replace(id.ToString(), newId.ToString());
        var tmpWikiTag = wikiTag.Replace(wiki.GetWikiType(), "temp");

        item.Text = item.Text.Replace(wikiTag, tmpWikiTag);

        mappedWikiTags.Add(tmpWikiTag, newWikiTag);
      }

      foreach (var key in mappedWikiTags.Keys)
      {
        GetLogger().LogDebug($"Remapping '{key}' -> '{mappedWikiTags[key]}'");
        item.Text = item.Text.Replace(key, mappedWikiTags[key]);
      }

      return rc;
    }

    /// <summary>
    /// Saves import object to database
    /// </summary>
    /// <param name="dtos">All import dtos (for lookups into related objects)</param>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save(int recordIndex, IEnumerable<dynamic> elements)
    {
      var item = _mapper.ElementsToPhys(elements);
      var oldId = item.Id;

      item.Id = 0;

      var mapDto = GetImporter().GetDto(Importer.DtoTypes.XmlMapDto);
      item.MapId = mapDto.GetIdTranslation(GetFileName(), item.MapId).Value;

      // remap 'true' MR's before Avatars since Avatars are rendered as MR's.
      RemapMrWikiTags(item);
      RemapQrWikiTags(item);
      ReplaceVpdWikiTags(item);
      ReplaceAvWikiTags(item);

      item.Info = $"\nImported from map_node.xml. id = {oldId}";

      Context.MapNodes.Add(item);
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);

      GetModel().Data.Add(item);

      GetLogger().LogDebug($"Saved {GetFileName()} id {oldId} -> {item.Id}");

      return true;
    }

  }

}