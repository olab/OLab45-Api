using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Importer
{
  /// <summary>
  /// Xml import base object
  /// </summary>
  /// <typeparam name="P">Model (physical) class</typeparam>
  public abstract class XmlImportDto<P> : XmlDto where P : new()
  {
    protected dynamic _phys;
    private readonly string _fileName;
    private readonly OLabLogger _olabLogger;
    private readonly Importer _importer;
    protected OLabDBContext Context;
    private readonly string _websitePublicFilesDirectory;
    private string _importDirectory;

    private readonly WikiTagProvider _tagProvider;
    public WikiTagProvider GetWikiProvider() { return _tagProvider; }

    protected readonly P _modelObject = new P();

    public override bool PostProcess(IDictionary<Importer.DtoTypes, XmlDto> dtos) { return true; }
    public Importer GetImporter() { return _importer; }
    public string GetFileName() { return _fileName; }
    public OLabLogger GetLogger() { return _olabLogger; }
    public P GetModel() { return _modelObject; }
    public dynamic GetXmlPhys() { return _phys; }
    public override Object GetDbPhys() { return _modelObject; }
    public abstract IEnumerable<dynamic> GetElements(dynamic xmlPhys);
    public string GetImportPackageDirectory() { return _importDirectory; }
    public string GetWebsitePublicDirectory() { return _websitePublicFilesDirectory; }
    public void SetImportDirectory(string dir) { _importDirectory = dir; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="importer">Importer object</param>
    /// <param name="fileName">File name to import</param>
    public XmlImportDto(Importer importer, string fileName)
    {
      _importer = importer;
      _fileName = fileName;
      _tagProvider = GetImporter().GetWikiProvider();
      _olabLogger = GetImporter().GetLogger();
      Context = GetImporter().GetContext();
      _websitePublicFilesDirectory = GetImporter().Settings.WebsitePublicFilesDirectory;
    }

    /// <summary>
    /// Get postimport object creation id
    /// </summary>
    /// <param name="originalId">Import system id</param>
    /// <returns></returns>
    public override uint? GetIdTranslation(string referencedFile, uint originalId)
    {
      if (originalId == 0)
        return 0;

      if (_idTranslation.TryGetValue(originalId, out uint? newId))
        return newId;

      throw new KeyNotFoundException($"references {GetFileName()} Id {originalId}: not found");
    }

    /// <summary>
    /// Get postimport object creation id
    /// </summary>
    /// <param name="originalId">(nullable) original id</param>
    /// <returns></returns>
    public override int? GetIdTranslation(string referencedFile, int? originalId)
    {
      if (!originalId.HasValue)
        return originalId;
      var value = GetIdTranslation(referencedFile, (uint)originalId.Value);
      return ( int? )value;
    }

    /// <summary>
    /// Load import files
    /// </summary>
    /// <param name="importDirectory">Directory containing import files</param>
    /// <returns>Success/Failure</returns>
    public override bool Load(string importDirectory)
    {
      bool rc = true;

      try
      {
        _importDirectory = importDirectory;

        string filePath = Path.Combine(GetImportPackageDirectory(), GetFileName());
        GetLogger().LogDebug($"Loading {GetFileName()}");

        if (File.Exists(filePath))
          _phys = DynamicXml.Load(filePath);
        else
        {
          GetLogger().LogDebug($"File {GetFileName()} does not exist");
          return false;
        }

        var outerElements = GetElements(GetXmlPhys());

        int record = 0;

        foreach (var innerElements in outerElements)
        {
          try
          {
            ++record;
            var elements = (IEnumerable<dynamic>)innerElements.Elements();
            xmlImportElementSets.Add(elements);
          }
          catch (Exception ex)
          {
            GetLogger().LogError(ex, $"Error loading '{GetFileName()}' record #{record}: {ex.Message}");
          }
        }

        GetLogger().LogDebug($"imported {xmlImportElementSets.Count()} {GetFileName()} objects");

      }
      catch (Exception ex)
      {
        if (!ex.Message.Contains("File not found"))
          GetLogger().LogError(ex, $"load error: {ex.Message}");
        rc = false;
      }

      return rc;
    }

    /// <summary>
    /// Save XmlDto's to physical database
    /// </summary>
    /// <param name="dtos">All import dtos</param>
    /// <returns>Success/Failure</returns>
    public override bool Save()
    {
      GetLogger().LogDebug($"Saving {xmlImportElementSets.Count()} {GetFileName()} objects");

      int recordIndex = 1;
      foreach (var elements in xmlImportElementSets)
      {
        try
        {
          Save(recordIndex, elements);
        }
        catch (System.Exception ex)
        {
          GetLogger().LogError($"Error {GetFileName()} record #{recordIndex}: {ex.Message}");
        }

        recordIndex++;
      }

      return true;
    }

    // implemented here so non-applicable derived classes
    // do not need to re-implement it
    public virtual bool Save(int recordIndex, IEnumerable<dynamic> elements)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate target directory for scoped type and id
    /// </summary>
    /// <param name="parentType">Scoped object type (e.g. 'Maps')</param>
    /// <param name="parentId">Scoped object id</param>
    /// <param name="path">Optional file name</param>
    /// <returns>Public directory for scope</returns>
    public string GetPublicFileDirectory(string parentType, uint parentId, string path = "")
    {
      var targetDirectory = Path.Combine(GetWebsitePublicDirectory(), parentType);
      targetDirectory = Path.Combine(targetDirectory, parentId.ToString());

      if (!string.IsNullOrEmpty(path))
        targetDirectory = Path.Combine(targetDirectory, path);

      return targetDirectory;
    }

    /// <summary>
    /// Test if public file exists
    /// </summary>
    /// <param name="path">Full path to file</param>
    /// <returns>true/false</returns>
    public bool DoesPublicFileExist(string path)
    {
      String filePath = Path.Combine(GetWebsitePublicDirectory(), Path.GetFileName(path));
      return File.Exists(filePath);
    }

    public static IList<string> GetWikiTags(string source)
    {
      var tags = new List<string>();
      return tags;
    }

    public static bool ReplaceWikiTags(string haystack, string needle, string newNeedle)
    {
      return true;
    }


  }

}