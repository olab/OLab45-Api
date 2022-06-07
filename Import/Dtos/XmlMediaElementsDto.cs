using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using static OLabWebAPI.Importer.Importer;

namespace OLabWebAPI.Importer
{

  public class XmlMediaElementsDto : XmlImportDto<XmlMediaElement>
  {
    public XmlMediaElementsDto(Importer importer) : base(importer, "media_elements.xml") { }

    /// <summary>
    /// Loads the specific import file into a model object
    /// </summary>
    /// <param name="importDirectory">Directory where import file exists</param>
    /// <returns></returns>
    public override bool Load(string importDirectory)
    {
      var result = base.Load(importDirectory);
      int record = 0;

      if (result)
      {
        try
        {
          var outerElements = (IEnumerable<dynamic>)GetXmlPhys().media_elements.media_elements_files.Elements();

          foreach (var element in outerElements)
          {
            try
            {
              ++record;
              var file = element.Value;
              file = Conversions.Base64Decode(element);
              GetModel().MediaElementsFiles.Add(file);
            }
            catch (Exception ex)
            {
              GetLogger().LogError(ex, $"Error loading '{GetFileName()}' media_elements_files record #{record}: {ex.Message}");
            }
          }
        }
        catch (RuntimeBinderException)
        {
          GetLogger().LogWarning($"No media_elements_files records in {GetFileName()}");
        }

        record = 0;
        
        try
        {
          var outerElements = (IEnumerable<dynamic>)GetXmlPhys().media_elements.media_elements_avatars.Elements();

          foreach (var element in outerElements)
          {
            try
            {
              record++;
              var file = element.Value;
              file = Conversions.Base64Decode(file);
              GetModel().MediaElementsAvatars.Add(file);
            }
            catch (Exception ex)
            {
              GetLogger().LogError(ex, $"Error loading '{GetFileName()}' media_elements_avatars record #{++record}: {ex.Message}");
            }

          }
        }
        catch (RuntimeBinderException)
        {
          GetLogger().LogWarning($"No media_elements_avatars records in {GetFileName()}");
        }

        GetLogger().LogDebug($"loaded {GetModel().MediaElementsFiles.Count()} {GetFileName()} MediaElementsFiles objects");
        GetLogger().LogDebug($"loaded {GetModel().MediaElementsAvatars.Count()} {GetFileName()} MediaElementsAvatars objects");

      }

      return result;
    }

    /// <summary>
    /// Extract records from the xml document
    /// </summary>
    /// <param name="importDirectory">Dynamic Xml object</param>
    /// <returns>Sets of element sets</returns>
    public override IEnumerable<dynamic> GetElements(dynamic xmlPhys)
    {
      return (IEnumerable<dynamic>)xmlPhys.media_elements.Elements();
    }


    /// <summary>
    /// Saves media files to public website directory
    /// </summary>
    /// <param name="elements">XML doc as an array of elements</param>
    /// <returns>Success/failure</returns>
    public override bool Save( int recordIndex, IEnumerable<dynamic> elements)
    {
      bool rc = true;

      try
      {
        var sourceDirectory = Path.Combine(GetImportPackageDirectory(), "media");

        var mapDto = GetImporter().GetDto(DtoTypes.XmlMapDto) as XmlMapDto;
        var map = mapDto.GetModel().Data.FirstOrDefault();

        var targetDirectory = GetPublicFileDirectory("Maps", map.Id);
        Directory.CreateDirectory(targetDirectory);

        foreach (var element in elements)
        {
          var fileName = Conversions.Base64Decode(element.Value);

          try
          {
            var sourceFileName = Path.Combine(sourceDirectory, fileName);
            var targetFileName = Path.Combine(targetDirectory, fileName);
            File.Copy(sourceFileName, targetFileName, true);

            GetLogger().LogDebug($"Copied {GetFileName()} '{fileName}' -> '{targetDirectory}'");

          }
          catch (System.Exception ex)
          {
            GetLogger().LogError($"Error importing {GetFileName()} '{fileName}': reason : {ex.Message}");
            rc = false;
          }

        }

      }
      catch (System.Exception ex)
      {
        GetLogger().LogError($"Error importing {GetFileName()}: reason : {ex.Message}");
        rc = false;
      }

      return rc;
    }
  }

}