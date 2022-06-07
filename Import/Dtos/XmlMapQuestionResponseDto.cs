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

  public class XmlMapQuestionResponseDto : XmlImportDto<XmlMapQuestionResponses>
  {
    private readonly ObjectMapper.QuestionResponses _mapper;

    public XmlMapQuestionResponseDto(Importer importer) : base(importer, "map_question_response.xml")
    {
      _mapper = new ObjectMapper.QuestionResponses(GetLogger(), GetWikiProvider());
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.map_question_response.Elements();
    }

    /// <summary>
    /// Saves import object to database
    /// </summary>
    /// <param name="dtos">All import dtos (for lookups into related objects)</param>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      var item = _mapper.ElementsToPhys(elements);
      var oldId = item.Id;
      
      GetLogger().LogDebug($"Saving {GetFileName()} id {oldId}");

      item.Id = 0;  

      var questionDto = GetImporter().GetDto( Importer.DtoTypes.XmlMapQuestionDto ) as XmlMapQuestionDto;
      item.QuestionId = questionDto.GetIdTranslation( GetFileName(), item.QuestionId.Value );
      item.Description = $"Imported from {GetFileName()} id = {oldId}";

      Context.SystemQuestionResponses.Add(item);
      Context.SaveChanges();

      // don't have a name, so save the id as the new name
      item.Name = item.Id.ToString();
      Context.SaveChanges();

      CreateIdTranslation(oldId, item.Id);
      GetLogger().LogDebug($"Saved {GetFileName()} id {oldId} -> {item.Id}");

      return true;
    }      
  }

}