using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLabWebAPI.Common;
using OLabWebAPI.Importer;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UserContext = OLabWebAPI.Data.UserContext;

namespace OLabWebAPI.Endpoints.WebApi
{
  [Route("olab/api/v3/[controller]/[action]")]
  [ApiController]
  public class ImportController : OlabController
  {
    private readonly IImporter _importer;
    private readonly AppSettings _appSettings;

    public ImportController(IOptions<AppSettings> appSettings, ILogger logger, OLabDBContext context) : base(logger, appSettings, context)
    {
      _appSettings = appSettings.Value;
      this.logger = new OLabLogger(logger);
      _importer = new Importer.Importer(_appSettings, this.logger, this.dbContext);
    }

    private string GetUploadDirectory()
    {
      return _appSettings.DefaultImportDirectory;
    }

    //[HttpPost("upload", Name = "upload")]
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);

        // test if user has access to import.
        var userContext = new UserContext(logger, dbContext, HttpContext);
        if (!userContext.HasAccess("X", "Import", 0))
          return OLabUnauthorizedObjectResult<uint>.Result(userContext.UserId);

        var fileName = await WriteFile(file);

        if (!CheckIfValidFile(fileName))
        {
          System.IO.File.Delete(fileName);
          throw new Exception("Invalid file");
        }

        logger.ClearMessages();

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
        Messages = logger.GetMessageStrings()
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
    public IActionResult Post(IFormFile file)
    {
      var auth = new OLabWebApiAuthorization(logger, dbContext, HttpContext);

      // test if user has access to map.
      Data.Interface.IUserContext userContext = auth.GetUserContext();
      if (!userContext.HasAccess("X", "Import", 0))
        return OLabUnauthorizedObjectResult<uint>.Result(userContext.UserId);

      // test for bad file name (including any directory characters)
      if (file.FileName.Contains(Path.DirectorySeparatorChar))
        logger.LogError("Invalid file name");
      else
      {
        var fullFileName = Path.Combine(GetUploadDirectory(), file.FileName);

        if (!System.IO.File.Exists(fullFileName))
          logger.LogError("Unable to load file");
        else
        {
          logger.LogInformation($"Loading archive: '{Path.GetFileName(fullFileName)}'");

          if (_importer.LoadAll(fullFileName))
            _importer.SaveAll();
        }
      }

      var dto = new ImportResponse { Messages = logger.GetMessageStrings() };

      return OLabObjectResult<ImportResponse>.Result(dto);
    }

    private bool CheckIfValidFile(string path)
    {
      var rc = true;

      try
      {
        using (ZipArchive zipFile = ZipFile.OpenRead(path))
        {
          System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entries = zipFile.Entries;
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
      var fileName = Path.GetRandomFileName();
      fileName += Path.GetExtension(file.FileName);

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

      return path;
    }

  }

}
