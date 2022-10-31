using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLabWebAPI.Common;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Utils;
using OLabWebAPI.Endpoints;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Services;

namespace OLabWebAPI.Controllers.Player
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
  {
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
      var factories = context.ValueProviderFactories;
      factories.RemoveType<FormValueProviderFactory>();
      factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
  }

  [Route("olab/api/v3/files")]
  [ApiController]
  public partial class FilesController : OlabController
  {
    private readonly AppSettings _appSettings;
    private readonly FilesEndpoint _endpoint;

    public FilesController(ILogger<CountersController> logger, IOptions<AppSettings> appSettings, OLabDBContext context) : base(logger, context)
    {
      _endpoint = new FilesEndpoint(this.logger, appSettings, context);
      _appSettings = appSettings.Value;
    }

    private string GetUploadDirectory()
    {
      return _appSettings.DefaultImportDirectory;
    }

    private string GetStaticFilesDirectory()
    {
      return _appSettings.WebsitePublicFilesDirectory;
    }

    private static string CapitalizeFirstLetter(string str)
    {

      if (str.Length == 0)
        return str;

      if (str.Length == 1)
        return char.ToUpper(str[0]).ToString();
      else
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Build static file file name
    /// </summary>
    /// <param name="dto">Files dto</param>
    /// <param name="postedFile">FormData</param>
    /// <returns>Static File name</returns>
    private string BuildStaticFileName(FilesFullDto dto, IFormFile postedFile)
    {
      string tempFileName;

      var fileName = ContentDispositionHeaderValue
          .Parse(postedFile.ContentDisposition)
          .FileName.Trim('"');

      var dirName = Path.Combine(
        GetStaticFilesDirectory(),
        CapitalizeFirstLetter(dto.ImageableType),
        dto.ImageableId.ToString());

      tempFileName = Path.Combine(dirName, fileName);

      logger.LogDebug($"Static file name: {tempFileName}");

      return tempFileName;
    }

    /// <summary>
    /// Save form file contents to staging directory
    /// </summary>
    /// <param name="postedFile">FormData</param>
    /// <returns>Temp file name</returns>
    private string SaveTemporaryFile(IFormFile postedFile)
    {
      string tempFileName;

      if (postedFile.Length > 0)
      {
        var fileName = ContentDispositionHeaderValue
            .Parse(postedFile.ContentDisposition)
            .FileName.Trim('"');

        if (!Directory.Exists(GetUploadDirectory()))
          throw new Exception("Unable to open staging directory.");

        tempFileName = Path.Combine(GetUploadDirectory(), fileName);
        logger.LogDebug($"Temporary file name: {tempFileName}");

        using (var fileStream = new FileStream(tempFileName, FileMode.Create))
        {
          postedFile.CopyTo(fileStream);
        }

        logger.LogDebug($"File uploaded Successfully");

        return tempFileName;
      }

      throw new Exception("file not received");
    }


    private void CopyToStaticDirectory(string tempFileName, string staticFileName)
    {
      if (!Directory.Exists(Path.GetDirectoryName(staticFileName)))
      {
        logger.LogDebug($"creating directory {Path.GetDirectoryName(staticFileName)}");
        Directory.CreateDirectory(Path.GetDirectoryName(staticFileName));
      }

      logger.LogDebug($"Copying file from {tempFileName} to {staticFileName}");
      System.IO.File.Copy(tempFileName, staticFileName, true);
    }

    private bool Exists(uint id)
    {
      return context.SystemFiles.Any(e => e.Id == id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
    {
      try
      {
        var pagedResult = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<FilesDto>.Result(pagedResult.Data, pagedResult.Remaining);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync(uint id)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        var dto = await _endpoint.GetAsync(auth, id);
        return OLabObjectResult<FilesFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Saves a file edit
    /// </summary>
    /// <param name="id">file id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] FilesFullDto dto)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        await _endpoint.PutAsync(auth, id, dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return NoContent();
    }

    /// <summary>
    /// Create new file
    /// </summary>
    /// <param name="dto">File data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync()
    {
      try
      {
        logger.LogDebug($"FilesController.PostAsync()");
        var dto = new FilesFullDto(Request.Form);

        var builder = new FilesFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        string tempFileName = SaveTemporaryFile(Request.Form.Files[0]);
        string staticFileName = BuildStaticFileName(dto, Request.Form.Files[0]);

        // save just the file name to the database
        phys.Path = Path.GetFileName(staticFileName);
        MimeTypes.TryGetMimeType(phys.Path, out var mimeType);
        phys.Mime = mimeType;

        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        dto = await _endpoint.PostAsync(auth, phys);
        
        // successful save to database, copy the file to the
        // final location
        CopyToStaticDirectory(tempFileName, staticFileName);
        return OLabObjectResult<FilesFullDto>.Result(dto);

      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteAsync(uint id)
    {
      try
      {
        var auth = new OLabWebApiAuthorization(logger, context, HttpContext);
        await _endpoint.DeleteAsync(auth, id);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

      return NoContent();
    }
  }

}
