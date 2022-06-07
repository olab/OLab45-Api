using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OLabWebAPI.Model;
using OLabWebAPI.Importer;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;
using OLabWebAPI.Dto;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using OLabWebAPI.Common;
using System.Threading;
using System;
using System.IO.Compression;

namespace OLabWebAPI.Controllers
{
  [Route("olab/api/v3/[controller]/[action]")]
  [ApiController]
  public class ImportController : OlabController
  {
    private readonly IImporter _importer;
    private readonly AppSettings _appSettings;

    public ImportController(IOptions<AppSettings> appSettings, ILogger logger, OLabDBContext context) : base(logger, context)
    {
      _appSettings = appSettings.Value;
      this.logger = new OLabLogger(logger);
      _importer = new Importer.Importer(_appSettings, this.logger, this.context);
    }

    private string GetUploadDirectory()
    {
      return _appSettings.DefaultImportDirectory;
    }

    [HttpPost("upload", Name = "upload")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UploadAsync(IFormFile file)
    {
      try
      {
        logger.LogInformation($"UploadAsync: file name '{file.FileName}', size {file.Length}");

        // test if user has access to import.
        var userContext = new UserContext(logger, context, HttpContext);
        if (!userContext.HasAccess("X", "Import", 0))
          return OLabUnauthorizedObjectResult<uint>.Result(userContext.UserId);

        var fileName = await WriteFile(file);

        if (!CheckIfValidFile(fileName))
        {
          System.IO.File.Delete(fileName);
          throw new Exception("Invalid file");
        }

        logger.LogInformation($"Loading archive: '{fileName}'");

        if (_importer.LoadAll(fileName))
          _importer.SaveAll();

      }
      catch (Exception ex)
      {
        logger.LogError(ex, $"UploadAsync excpetion");
        return BadRequest(new { message = ex.Message });
      }

      var dto = new ImportResponse
      {
        Messages = logger.GetMessages()
      };

      return OLabObjectResult<ImportResponse>.Result(dto);
    }

    /// <summary>
    /// Runs an import
    /// </summary>
    /// <param name="request">ImportRequest</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Post(ImportRequest request)
    {
      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("X", "Import", 0))
        return OLabUnauthorizedObjectResult<uint>.Result(userContext.UserId);

      // test for bad file name (including any directory characters)
      if (request.FileName.Contains(Path.DirectorySeparatorChar))
        logger.LogError("Invalid file name");
      else
      {
        var fullFileName = Path.Combine(GetUploadDirectory(), request.FileName);

        if (!System.IO.File.Exists(fullFileName))
          logger.LogError("Unable to load file");
        else
        {
          logger.LogInformation($"Loading archive: '{Path.GetFileName(fullFileName)}'");

          if (_importer.LoadAll(fullFileName))
            _importer.SaveAll();
        }
      }

      var dto = new ImportResponse
      {
        Messages = logger.GetMessages(request.MessageLevel)
      };

      return OLabObjectResult<ImportResponse>.Result(dto);
    }

    private bool CheckIfValidFile(string path)
    {
      bool rc = true;

      try
      {
        using (var zipFile = ZipFile.OpenRead(path))
        {
          var entries = zipFile.Entries;
        }
      }
      catch (InvalidDataException)
      {
        rc = false;
      }

      logger.LogInformation($"Export file '{path}' valid? {rc}");
      return rc;
    }

    private async Task<string> WriteFile(IFormFile file)
    {
      // strip off any directory
      string fileName = Path.GetRandomFileName();

      var pathBuilt = GetUploadDirectory();
      if (!Directory.Exists(pathBuilt))
      {
        Directory.CreateDirectory(pathBuilt);
      }

      var path = Path.Combine(GetUploadDirectory(), fileName);

      using (var stream = new FileStream(path, FileMode.Create))
      {
        await file.CopyToAsync(stream);
        logger.LogInformation($"Wrote upload file to '{path}'. Size: {file.Length}");
      }

      return fileName;
    }

  }

}
