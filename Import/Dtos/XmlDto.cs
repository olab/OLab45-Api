using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{
  public abstract class XmlDto
  {
    public abstract bool Load(string importDirectory);
    public abstract bool PostProcess(IDictionary<Importer.DtoTypes, XmlDto> dtos);
    public abstract bool Save();
    public abstract object GetDbPhys();

    // used to hold on to id translation between origin system and new one
    protected IDictionary<uint, uint?> _idTranslation = new Dictionary<uint, uint?>();
    protected IList<IEnumerable<dynamic>> xmlImportElementSets = new List<IEnumerable<dynamic>>();

    /// <summary>
    /// Add id translation record to store
    /// </summary>
    /// <param name="originalId">Import system Id</param>
    /// <param name="newId">Database id</param>
    protected void CreateIdTranslation( uint originalId, uint? newId = null ) 
    {
      if ( _idTranslation.ContainsKey( originalId ))
        return;
      _idTranslation.Add( originalId, newId );
    }

    public abstract uint? GetIdTranslation( string referencedFile, uint originalId );
    public abstract int? GetIdTranslation( string referencedFile, int? originalId );
  }

}